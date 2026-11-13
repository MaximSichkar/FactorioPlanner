
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Application.Services;
using FactorioPlanner.Domain.Models;
using FactorioPlanner.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactorioPlanner.Web.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly IFactorioDataService dataService;
        private readonly ProductionCalculationService calculationService;

        public CalculatorController(
            IFactorioDataService dataService,
            ProductionCalculationService calculationService)
        {
            this.dataService = dataService;
            this.calculationService = calculationService;
        }

        [HttpGet]
        public IActionResult Index(string? recipeId)
        {
            CalculatorViewModel model = new CalculatorViewModel();

            List<Recipe> recipes = dataService.GetRecipes();

            if (string.IsNullOrWhiteSpace(recipeId))
            {
                Recipe? defaultRecipe = dataService.GetRecipeById(
                    "electronic-circuit"
                );

                if (defaultRecipe != null)
                {
                    recipeId = defaultRecipe.Id;
                }
                else if (recipes.Count > 0)
                {
                    recipeId = recipes[0].Id;
                }
            }

            model.RecipeId = recipeId ?? string.Empty;

            PopulateOptions(model);

            if (model.Factories.Count > 0)
            {
                model.FactoryId = model.Factories[0].Value ?? string.Empty;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CalculatorViewModel model)
        {
            PopulateOptions(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Recipe? recipe = dataService.GetRecipeById(model.RecipeId);

            Factory? factory = dataService.GetFactoryById(model.FactoryId);

            if (recipe == null || factory == null)
            {
                model.ErrorMessage = "Рецепт или завод не найден.";
                return View(model);
            }

            if (!factory.CanCraft(recipe))
            {
                model.ErrorMessage =
                    "Выбранный завод не поддерживает этот рецепт.";

                return View(model);
            }

            try
            {
                model.Result = calculationService.Calculate(
                    recipe,
                    factory,
                    recipe.Id,
                    model.TargetProductionPerSecond
                );
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException ||
                exception is OverflowException)
            {
                model.ErrorMessage = exception.Message;
            }

            return View(model);
        }

        private void PopulateOptions(CalculatorViewModel model)
        {
            List<Recipe> recipes = dataService.GetRecipes();

            foreach (Recipe recipe in recipes)
            {
                model.Recipes.Add(new SelectListItem
                {
                    Value = recipe.Id,
                    Text = recipe.Name,
                    Selected = recipe.Id == model.RecipeId
                });
            }

            Recipe? selectedRecipe =
                dataService.GetRecipeById(model.RecipeId);

            if (selectedRecipe == null)
            {
                return;
            }

            List<Factory> factories = dataService.GetFactories();

            foreach (Factory factory in factories)
            {
                if (!factory.CanCraft(selectedRecipe))
                {
                    continue;
                }

                model.Factories.Add(new SelectListItem
                {
                    Value = factory.Id,
                    Text = factory.Name,
                    Selected = factory.Id == model.FactoryId
                });
            }
        }
    }
}
