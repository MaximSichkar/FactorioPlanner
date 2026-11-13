
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Application.Services;
using FactorioPlanner.Domain.Models;
using Moq;

namespace FactorioPlanner.UnitTests
{
    public class ProductionBlockCalculationServiceTests
    {
        [Fact]
        public void Calculate_TwoSteps_ReturnsTwoResults()
        {
            // Arrange
            Mock<IFactorioDataService> dataMock = CreateDataMock();

            ProductionBlock block = new ProductionBlock
            {
                Name = "Electronic Circuits"
            };

            block.Steps.Add(new ProductionStep
            {
                RecipeId = "electronic-circuit",
                FactoryId = "assembling-machine-3",
                TargetProductionPerSecond = 10,
                Order = 0
            });

            block.Steps.Add(new ProductionStep
            {
                RecipeId = "copper-cable",
                FactoryId = "assembling-machine-3",
                TargetProductionPerSecond = 20,
                Order = 1
            });

            ProductionBlockCalculationService service =
                new ProductionBlockCalculationService(
                    dataMock.Object,
                    new ProductionCalculationService()
                );

            // Act
            var result = service.Calculate(block);

            // Assert
            Assert.Equal(2, result.Steps.Count);
            Assert.Equal(8, result.TotalFactoryCount);
            Assert.Equal(
                "electronic-circuit",
                result.Steps[0].Calculation.RecipeId
            );
            Assert.Equal(
                "copper-cable",
                result.Steps[1].Calculation.RecipeId
            );
        }

        [Fact]
        public void Calculate_MissingRecipe_ThrowsException()
        {
            // Arrange
            Mock<IFactorioDataService> dataMock =
                new Mock<IFactorioDataService>();

            ProductionBlock block = new ProductionBlock();

            block.Steps.Add(new ProductionStep
            {
                RecipeId = "unknown-recipe",
                FactoryId = "assembling-machine-3",
                TargetProductionPerSecond = 10
            });

            ProductionBlockCalculationService service =
                new ProductionBlockCalculationService(
                    dataMock.Object,
                    new ProductionCalculationService()
                );

            // Act
            Action action = delegate
            {
                service.Calculate(block);
            };

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void Calculate_EmptyBlock_ReturnsZeroFactories()
        {
            // Arrange
            Mock<IFactorioDataService> dataMock =
                new Mock<IFactorioDataService>();

            ProductionBlock block = new ProductionBlock();

            ProductionBlockCalculationService service =
                new ProductionBlockCalculationService(
                    dataMock.Object,
                    new ProductionCalculationService()
                );

            // Act
            var result = service.Calculate(block);

            // Assert
            Assert.Empty(result.Steps);
            Assert.Equal(0, result.TotalFactoryCount);
        }

        private static Mock<IFactorioDataService> CreateDataMock()
        {
            Recipe circuitRecipe = new Recipe
            {
                Id = "electronic-circuit",
                CraftTime = 0.5,
                Categories = new List<string> { "crafting" }
            };

            circuitRecipe.Products.Add(new RecipeItem
            {
                ItemId = "electronic-circuit",
                Amount = 1
            });

            Recipe cableRecipe = new Recipe
            {
                Id = "copper-cable",
                CraftTime = 0.5,
                Categories = new List<string> { "crafting" }
            };

            cableRecipe.Products.Add(new RecipeItem
            {
                ItemId = "copper-cable",
                Amount = 2
            });

            Factory factory = new Factory
            {
                Id = "assembling-machine-3",
                CraftingSpeed = 1.25,
                SupportedCategories = new List<string> { "crafting" }
            };

            Mock<IFactorioDataService> mock =
                new Mock<IFactorioDataService>();

            mock.Setup(service => service.GetRecipeById("electronic-circuit"))
                .Returns(circuitRecipe);

            mock.Setup(service => service.GetRecipeById("copper-cable"))
                .Returns(cableRecipe);

            mock.Setup(service => service.GetFactoryById("assembling-machine-3"))
                .Returns(factory);

            return mock;
        }
    }
}
