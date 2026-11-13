
namespace FactorioPlanner.Domain.Models
{
    public class Factory
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public double CraftingSpeed { get; set; } = 1.0;

        public int ModuleSlots
        {
            get; set;
        }

        public double BaseProductivity
        {
            get; set;
        }

        public List<string> SupportedCategories { get; set; } = new List<string>();

        public bool CanCraft(Recipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            if (SupportedCategories == null || SupportedCategories.Count == 0)
            {
                return false;
            }

            if (recipe.Categories == null || recipe.Categories.Count == 0)
            {
                return false;
            }

            foreach (string recipeCategory in recipe.Categories)
            {
                foreach (string supportedCategory in SupportedCategories)
                {
                    if (string.Equals(
                        recipeCategory,
                        supportedCategory,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
