
namespace FactorioPlanner.Domain.Models
{
    public class ProductionStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string RecipeId { get; set; } = string.Empty;

        public string FactoryId { get; set; } = string.Empty;
        public string TargetProductId { get; set; } = string.Empty;

        public double TargetProductionPerSecond
        {
            get; set;
        }

        public int Order
        {
            get; set;
        }
    }
}
