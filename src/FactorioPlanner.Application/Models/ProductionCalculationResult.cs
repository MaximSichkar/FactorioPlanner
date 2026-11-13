
namespace FactorioPlanner.Application.Models
{
    public class ProductionCalculationResult
    {
        public string RecipeId { get; set; } = string.Empty;

        public string FactoryId { get; set; } = string.Empty;

        public string TargetProductId { get; set; } = string.Empty;

        public double TargetProductionPerSecond
        {
            get; set;
        }

        public double ProductionPerFactory
        {
            get; set;
        }

        public double ExactFactoryCount
        {
            get; set;
        }

        public int FactoryCount
        {
            get; set;
        }

        public double MaximumProduction
        {
            get; set;
        }

        public double BaseProductivity
        {
            get; set;
        }

        public List<IngredientRate> Ingredients
        {
            get; set;
        } =
            new List<IngredientRate>();
    }
}
