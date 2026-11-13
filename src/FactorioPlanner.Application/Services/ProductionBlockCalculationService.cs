
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Application.Models;
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.Application.Services
{
    public class ProductionBlockCalculationService
    {
        private readonly IFactorioDataService dataService;
        private readonly ProductionCalculationService calculationService;

        public ProductionBlockCalculationService(
            IFactorioDataService dataService,
            ProductionCalculationService calculationService)
        {
            this.dataService = dataService;
            this.calculationService = calculationService;
        }

        public ProductionBlockResult Calculate(ProductionBlock block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            ProductionBlockResult result = new ProductionBlockResult
            {
                BlockId = block.Id,
                BlockName = block.Name
            };

            List<ProductionStep> orderedSteps = new List<ProductionStep>(
                block.Steps
            );

            orderedSteps.Sort(CompareSteps);

            foreach (ProductionStep step in orderedSteps)
            {
                Recipe? recipe = dataService.GetRecipeById(step.RecipeId);

                if (recipe == null)
                {
                    throw new InvalidOperationException(
                        "Рецепт не найден: " + step.RecipeId
                    );
                }

                Factory? factory = dataService.GetFactoryById(step.FactoryId);

                if (factory == null)
                {
                    throw new InvalidOperationException(
                        "Машина не найдена: " + step.FactoryId
                    );
                }


                string targetProductId = string.IsNullOrWhiteSpace(step.TargetProductId)
                    ? step.RecipeId
                    : step.TargetProductId;

                ProductionCalculationResult calculation =
                    calculationService.Calculate(
                        recipe,
                        factory,
                        targetProductId,
                        step.TargetProductionPerSecond
                    );


                ProductionStepResult stepResult = new ProductionStepResult
                {
                    StepId = step.Id,
                    Order = step.Order,
                    Calculation = calculation
                };

                result.Steps.Add(stepResult);

                result.TotalFactoryCount = checked(
                    result.TotalFactoryCount + calculation.FactoryCount
                );
            }

            return result;
        }

        private static int CompareSteps(
            ProductionStep first,
            ProductionStep second)
        {
            return first.Order.CompareTo(second.Order);
        }
    }
}
