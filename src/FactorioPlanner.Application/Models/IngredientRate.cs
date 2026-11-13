
namespace FactorioPlanner.Application.Models
{
    public class IngredientRate
    {
        public string ItemId { get; set; } = string.Empty;

        public bool IsFluid
        {
            get; set;
        }

        public double RequiredPerSecond
        {
            get; set;
        }

        public double MaximumPerSecond
        {
            get; set;
        }
    }
}
