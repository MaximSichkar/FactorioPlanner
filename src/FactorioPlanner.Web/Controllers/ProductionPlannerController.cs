
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Application.Services;
using FactorioPlanner.Domain.Models;
using FactorioPlanner.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactorioPlanner.Web.Controllers
{
    public class ProductionPlannerController : Controller
    {
        private readonly IFactorioDataService dataService;
        private readonly ProductionBlockCalculationService blockService;

        public ProductionPlannerController(
            IFactorioDataService dataService,
            ProductionBlockCalculationService blockService)
        {
            this.dataService = dataService;
            this.blockService = blockService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ProductionPlannerViewModel model =
                new ProductionPlannerViewModel();

            ProductionStepViewModel step = CreateDefaultStep();

            model.Steps.Add(step);

            PopulateOptions(model);
            ModelState.Clear();

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(
            ProductionPlannerViewModel model,
            string command,
            int rowIndex = -1,
            string ingredientItemId = "",
            double ingredientRate = 0,
            string selectedRecipeId = "")
        {
            model.Steps ??= new List<ProductionStepViewModel>();

            if (model.Steps.Count > 100)
            {
                model.ErrorMessage =
                    "В одном блоке допускается не более 100 рецептов.";

                PopulateOptions(model);
                ModelState.Clear();

                return View(model);
            }

            if (command == "add")
            {
                if (model.Steps.Count >= 100)
                {
                    model.ErrorMessage =
                        "В одном блоке допускается не более 100 рецептов.";
                }
                else
                {
                    model.Steps.Add(CreateDefaultStep());
                }
            }
            else if (command == "delete")
            {
                if (rowIndex >= 0 && rowIndex < model.Steps.Count)
                {
                    model.Steps.RemoveAt(rowIndex);
                }
            }

            else if (command == "addIngredient")
            {
                try
                {
                    if (rowIndex < 0 || rowIndex >= model.Steps.Count)
                    {
                        throw new ArgumentException(
                            "Не удалось определить исходную строку."
                        );
                    }

                    if (string.IsNullOrWhiteSpace(ingredientItemId) ||
                        !double.IsFinite(ingredientRate) ||
                        ingredientRate <= 0)
                    {
                        throw new ArgumentException(
                            "Некорректные данные ингредиента."
                        );
                    }

                    List<Recipe> candidates =
                        dataService.GetRecipesProducingItem(ingredientItemId);

                    List<Recipe> availableRecipes = new List<Recipe>();


                    foreach (Recipe recipe in candidates)
                    {
                        if (IsRecyclingRecipe(recipe))
                        {
                            continue;
                        }

                        if (FindFirstCompatibleFactory(recipe) != null)
                        {
                            availableRecipes.Add(recipe);
                        }
                    }


                    if (availableRecipes.Count == 0)
                    {
                        throw new ArgumentException(
                            "Нет доступных способов производства: " +
                            ingredientItemId
                        );
                    }

                    if (availableRecipes.Count == 1)
                    {
                        AddIngredientStep(
                            model,
                            rowIndex,
                            ingredientItemId,
                            ingredientRate,
                            availableRecipes[0].Id
                        );
                    }
                    else
                    {
                        model.ShowRecipeChoice = true;
                        model.PendingIngredientItemId = ingredientItemId;
                        model.PendingIngredientRate = ingredientRate;
                        model.PendingRowIndex = rowIndex;

                        foreach (Recipe recipe in availableRecipes)
                        {
                            model.AlternativeRecipes.Add(new SelectListItem
                            {
                                Value = recipe.Id,
                                Text = recipe.Name
                            });
                        }
                    }
                }
                catch (ArgumentException exception)
                {
                    model.ErrorMessage = exception.Message;
                }
            }
            else if (command == "chooseIngredientRecipe")
            {
                try
                {
                    AddIngredientStep(
                        model,
                        rowIndex,
                        ingredientItemId,
                        ingredientRate,
                        selectedRecipeId
                    );
                }
                catch (ArgumentException exception)
                {
                    model.ErrorMessage = exception.Message;
                }
            }


            PopulateOptions(model);
            ModelState.Clear();

            if (model.ErrorMessage == null && !model.ShowRecipeChoice)
            {
                try
                {
                    ProductionBlock block = CreateBlock(model);
                    model.Result = blockService.Calculate(block);
                }
                catch (Exception exception) when (
                    exception is ArgumentException ||
                    exception is InvalidOperationException ||
                    exception is OverflowException)
                {
                    model.ErrorMessage = exception.Message;
                }
            }

            return View(model);
        }




        private void AddIngredientStep(
            ProductionPlannerViewModel model,
            int rowIndex,
            string ingredientItemId,
            double ingredientRate,
            string selectedRecipeId)
        {
            if (rowIndex < 0 || rowIndex >= model.Steps.Count)
            {
                throw new ArgumentException(
                    "Не найдена исходная строка."
                );
            }

            if (string.IsNullOrWhiteSpace(ingredientItemId) ||
                !double.IsFinite(ingredientRate) ||
                ingredientRate <= 0)
            {
                throw new ArgumentException(
                    "Некорректное количество ингредиента."
                );
            }

            Recipe? recipe = dataService.GetRecipeById(selectedRecipeId);

            if (recipe == null)
            {
                throw new ArgumentException(
                    "Выбранный рецепт не найден."
                );
            }

            bool producesTarget = false;

            foreach (RecipeItem product in recipe.Products)
            {
                if (string.Equals(
                    product.ItemId,
                    ingredientItemId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    producesTarget = true;
                    break;
                }
            }

            if (!producesTarget)
            {
                throw new ArgumentException(
                    "Этот рецепт не производит выбранный ингредиент."
                );
            }

            // Защита от дублирования выбранного рецепта.
            foreach (ProductionStepViewModel step in model.Steps)
            {
                if (string.Equals(
                    step.RecipeId,
                    recipe.Id,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (step.TargetProductionPerSecond < ingredientRate)
                    {
                        step.TargetProductionPerSecond = ingredientRate;
                    }

                    step.TargetProductId = ingredientItemId;

                    return;
                }
            }

            if (model.Steps.Count >= 100)
            {
                throw new ArgumentException(
                    "В одном блоке допускается не более 100 рецептов."
                );
            }

            Factory? factory = FindFirstCompatibleFactory(recipe);

            if (factory == null)
            {
                throw new ArgumentException(
                    "Нет совместимой машины для рецепта: " + recipe.Id
                );
            }

            ProductionStepViewModel newStep = new ProductionStepViewModel
            {
                RecipeId = recipe.Id,
                TargetProductId = ingredientItemId,
                FactoryId = factory.Id,
                TargetProductionPerSecond = ingredientRate
            };

            model.Steps.Insert(rowIndex + 1, newStep);
        }



        private ProductionStepViewModel CreateDefaultStep()
        {
            List<Recipe> recipes = dataService.GetRecipes();

            Recipe? recipe = dataService.GetRecipeById(
                "electronic-circuit"
            );

            if (recipe == null && recipes.Count > 0)
            {
                recipe = recipes[0];
            }

            ProductionStepViewModel step =
                new ProductionStepViewModel();

            if (recipe != null)
            {
                step.RecipeId = recipe.Id;

                Factory? factory = FindFirstCompatibleFactory(recipe);

                if (factory != null)
                {
                    step.FactoryId = factory.Id;
                }
            }

            return step;
        }

        private Recipe? FindPreferredRecipeForItem(string itemId)
        {
            List<Recipe> candidates = new List<Recipe>();

            foreach (Recipe recipe in dataService.GetRecipes())
            {
                if (!RecipeProducesItem(recipe, itemId))
                {
                    continue;
                }

                if (!IsAutoExpandableRecipe(recipe))
                {
                    continue;
                }

                candidates.Add(recipe);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            candidates.Sort(CompareAutoExpandRecipes);

            return candidates[0];
        }

        private static bool RecipeProducesItem(Recipe recipe, string itemId)
        {
            foreach (RecipeItem product in recipe.Products)
            {
                if (string.Equals(
                    product.ItemId,
                    itemId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAutoExpandableRecipe(Recipe recipe)
        {
            if (recipe.Id.EndsWith(
                "-recycling",
                StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(
                    recipe.Id,
                    "scrap-recycling",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static int CompareAutoExpandRecipes(
            Recipe first,
            Recipe second)
        {
            bool firstExact = string.Equals(
                first.Id,
                first.Products.Count > 0 ? first.Products[0].ItemId : string.Empty,
                StringComparison.OrdinalIgnoreCase);

            bool secondExact = string.Equals(
                second.Id,
                second.Products.Count > 0 ? second.Products[0].ItemId : string.Empty,
                StringComparison.OrdinalIgnoreCase);

            if (firstExact && !secondExact)
            {
                return -1;
            }

            if (!firstExact && secondExact)
            {
                return 1;
            }

            return string.Compare(
                first.Id,
                second.Id,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRecyclingRecipe(Recipe recipe)
        {
            if (string.Equals(
                recipe.Id,
                "scrap-recycling",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (recipe.Id.EndsWith(
                "-recycling",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (string category in recipe.Categories)
            {
                if (string.Equals(
                    category,
                    "recycling",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        private Factory? FindFirstCompatibleFactory(Recipe recipe)
        {
            foreach (Factory factory in dataService.GetFactories())
            {
                if (factory.CanCraft(recipe))
                {
                    return factory;
                }
            }

            return null;
        }

        private void PopulateOptions(ProductionPlannerViewModel model)
        {
            model.Recipes.Clear();

            foreach (Recipe recipe in dataService.GetRecipes())
            {
                model.Recipes.Add(new SelectListItem
                {
                    Value = recipe.Id,
                    Text = recipe.Name
                });
            }

            foreach (ProductionStepViewModel step in model.Steps)
            {
                step.Factories.Clear();

                Recipe? recipe = dataService.GetRecipeById(
                    step.RecipeId
                );

                if (recipe == null)
                {
                    continue;
                }

                foreach (Factory factory in dataService.GetFactories())
                {
                    if (!factory.CanCraft(recipe))
                    {
                        continue;
                    }

                    step.Factories.Add(new SelectListItem
                    {
                        Value = factory.Id,
                        Text = factory.Name
                    });
                }

                bool factoryIsAvailable = false;

                foreach (SelectListItem factory in step.Factories)
                {
                    if (factory.Value == step.FactoryId)
                    {
                        factoryIsAvailable = true;
                        break;
                    }
                }

                if (!factoryIsAvailable)
                {
                    step.FactoryId = step.Factories.Count > 0
                        ? step.Factories[0].Value ?? string.Empty
                        : string.Empty;
                }
            }
        }

        private static ProductionBlock CreateBlock(
            ProductionPlannerViewModel model)
        {
            ProductionBlock block = new ProductionBlock
            {
                Id = model.BlockId,
                Name = model.Name
            };

            for (int i = 0; i < model.Steps.Count; i++)
            {
                ProductionStepViewModel source = model.Steps[i];

                if (string.IsNullOrWhiteSpace(source.RecipeId) ||
                    string.IsNullOrWhiteSpace(source.FactoryId))
                {
                    throw new ArgumentException(
                        "Выберите рецепт и совместимую машину для каждой строки."
                    );
                }

                if (!double.IsFinite(source.TargetProductionPerSecond) ||
                    source.TargetProductionPerSecond <= 0)
                {
                    throw new ArgumentException(
                        "Выпуск каждого рецепта должен быть больше нуля."
                    );
                }


                block.Steps.Add(new ProductionStep
                {
                    Id = source.Id,
                    RecipeId = source.RecipeId,
                    TargetProductId = string.IsNullOrWhiteSpace(source.TargetProductId)
                        ? source.RecipeId
                        : source.TargetProductId,
                    FactoryId = source.FactoryId,
                    TargetProductionPerSecond = source.TargetProductionPerSecond,
                    Order = i
                });

            }

            return block;
        }
    }
}
