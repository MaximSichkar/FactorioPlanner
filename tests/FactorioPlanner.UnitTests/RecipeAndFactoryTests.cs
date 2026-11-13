
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.UnitTests
{
    public class RecipeAndFactoryTests
    {
        [Fact]
        public void Recipe_ShouldSupportMultipleProducts()
        {
            // Arrange
            Recipe recipe = new Recipe
            {
                Id = "test-oil-processing"
            };

            // Act
            recipe.Products.Add(new RecipeItem
            {
                ItemId = "petroleum-gas",
                Amount = 55,
                IsFluid = true
            });

            recipe.Products.Add(new RecipeItem
            {
                ItemId = "heavy-oil",
                Amount = 25,
                IsFluid = true
            });

            // Assert
            Assert.Equal(2, recipe.Products.Count);
            Assert.All(recipe.Products, AssertFluid);
        }

        [Fact]
        public void Factory_ShouldAcceptCompatibleRecipe()
        {
            // Arrange
            Recipe recipe = new Recipe
            {
                Id = "electronic-circuit",
                Categories = new List<string>
                {
                    "crafting",
                    "electromagnetics"
                }
            };

            Factory factory = new Factory
            {
                Id = "electromagnetic-plant",
                SupportedCategories = new List<string>
                {
                    "electromagnetics"
                }
            };

            // Act
            bool result = factory.CanCraft(recipe);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Factory_ShouldRejectIncompatibleRecipe()
        {
            // Arrange
            Recipe recipe = new Recipe
            {
                Id = "electronic-circuit",
                Categories = new List<string>
                {
                    "crafting"
                }
            };

            Factory factory = new Factory
            {
                Id = "crusher",
                SupportedCategories = new List<string>
                {
                    "crushing"
                }
            };

            // Act
            bool result = factory.CanCraft(recipe);

            // Assert
            Assert.False(result);
        }

        private static void AssertFluid(RecipeItem item)
        {
            Assert.True(item.IsFluid);
        }
    }
}
