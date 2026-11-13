
namespace FactorioPlanner.Domain.Models
{
    public class RecipeItem
    {
        public string ItemId { get; set; } = string.Empty;

        public double Amount
        {
            get; set;
        }

        public bool IsFluid
        {
            get; set;
        }
    }
}
