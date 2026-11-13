
namespace FactorioPlanner.Domain.Models
{
    public class Recipe
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public double CraftTime { get; set; } = 1.0;

        public List<string> Categories { get; set; } = new List<string>();

        public List<RecipeItem> Ingredients { get; set; } = new List<RecipeItem>();

        public List<RecipeItem> Products { get; set; } = new List<RecipeItem>();

        public bool AllowProductivity
        {
            get; set;
        }
    }
}
