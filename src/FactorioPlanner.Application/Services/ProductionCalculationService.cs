
using System;
using FactorioPlanner.Application.Models;
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.Application.Services
{
    public class ProductionCalculationService
    {
        public ProductionCalculationResult Calculate(
            Recipe recipe,
            Factory factory,
            string targetProductId,
            double targetProductionPerSecond)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (string.IsNullOrWhiteSpace(targetProductId))
            {
                throw new ArgumentException(
                    "Не указан целевой продукт.",
                    nameof(targetProductId)
                );
            }

            if (!double.IsFinite(targetProductionPerSecond) ||
                targetProductionPerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetProductionPerSecond),
                    "Требуемое производство должно быть положительным."
                );
            }

            if (!double.IsFinite(recipe.CraftTime) ||
                recipe.CraftTime <= 0)
            {
                throw new ArgumentException(
                    "Время производства должно быть положительным."
                );
            }

            if (!double.IsFinite(factory.CraftingSpeed) ||
                factory.CraftingSpeed <= 0)
            {
                throw new ArgumentException(
                    "Скорость машины должна быть положительной."
                );
            }

            if (!double.IsFinite(factory.BaseProductivity) ||
                factory.BaseProductivity < 0)
            {
                throw new ArgumentException(
                    "Некорректная встроенная продуктивность."
                );
            }

            if (!factory.CanCraft(recipe))
            {
                throw new InvalidOperationException(
                    "Выбранная машина не поддерживает этот рецепт."
                );
            }

            RecipeItem targetProduct = null;

            foreach (RecipeItem product in recipe.Products)
            {
                if (product.ItemId == targetProductId)
                {
                    targetProduct = product;
                    break;
                }
            }

            if (targetProduct == null)
            {
                throw new InvalidOperationException(
                    "Целевой продукт отсутствует в результатах рецепта."
                );
            }

            if (!double.IsFinite(targetProduct.Amount) ||
                targetProduct.Amount <= 0)
            {
                throw new ArgumentException(
                    "Количество целевого продукта должно быть положительным."
                );
            }

            double cyclesPerSecond =
                factory.CraftingSpeed / recipe.CraftTime;

            double productivityMultiplier =
                1.0 + factory.BaseProductivity;

            double productionPerFactory =
                cyclesPerSecond *
                targetProduct.Amount *
                productivityMultiplier;

            double exactFactoryCount =
                targetProductionPerSecond / productionPerFactory;

            if (!double.IsFinite(exactFactoryCount) ||
                exactFactoryCount > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetProductionPerSecond),
                    "Требуемое количество машин выходит за допустимые пределы."
                );
            }

            int factoryCount = (int)Math.Ceiling(exactFactoryCount);

            double maximumProduction =
                productionPerFactory * factoryCount;

            ProductionCalculationResult result =
                new ProductionCalculationResult
                {
                    RecipeId = recipe.Id,
                    FactoryId = factory.Id,
                    TargetProductId = targetProductId,
                    TargetProductionPerSecond = targetProductionPerSecond,
                    ProductionPerFactory = productionPerFactory,
                    ExactFactoryCount = exactFactoryCount,
                    FactoryCount = factoryCount,
                    MaximumProduction = maximumProduction,
                    BaseProductivity = factory.BaseProductivity
                };

            double requiredCyclesPerSecond =
                targetProductionPerSecond /
                (targetProduct.Amount * productivityMultiplier);

            double maximumCyclesPerSecond =
                cyclesPerSecond * factoryCount;

            foreach (RecipeItem ingredient in recipe.Ingredients)
            {
                if (!double.IsFinite(ingredient.Amount) ||
                    ingredient.Amount <= 0)
                {
                    throw new ArgumentException(
                        "Количество ингредиента должно быть положительным."
                    );
                }

                IngredientRate rate = new IngredientRate
                {
                    ItemId = ingredient.ItemId,
                    IsFluid = ingredient.IsFluid,
                    RequiredPerSecond =
                        ingredient.Amount * requiredCyclesPerSecond,
                    MaximumPerSecond =
                        ingredient.Amount * maximumCyclesPerSecond
                };

                result.Ingredients.Add(rate);
            }

            return result;
        }
    }
}
