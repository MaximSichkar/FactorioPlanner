
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Application.Models;
using FactorioPlanner.Application.Services;
using FactorioPlanner.Domain.Models;
using Moq;
using System;
using System.Collections.Generic;

namespace FactorioPlanner.UnitTests
{
    public class ProductionCalculationServiceTests
    {
        [Fact]
        public void Calculate_ElectromagneticPlant_ReturnsCorrectResult()
        {
            // Arrange
            Recipe recipe = CreateElectronicCircuitRecipe();
            Factory factory = CreateElectromagneticPlant();

            ProductionCalculationService service =
                new ProductionCalculationService();

            // Act
            ProductionCalculationResult result = service.Calculate(
                recipe,
                factory,
                "electronic-circuit",
                10
            );

            // Assert
            Assert.Equal(6.0, result.ProductionPerFactory, 6);
            Assert.Equal(2, result.FactoryCount);
            Assert.Equal(12.0, result.MaximumProduction, 6);
            Assert.Equal(0.5, result.BaseProductivity, 6);

            Assert.Equal(2, result.Ingredients.Count);

            Assert.Equal(
                6.666667,
                result.Ingredients[0].RequiredPerSecond,
                5
            );

            Assert.Equal(
                20.0,
                result.Ingredients[1].RequiredPerSecond,
                6
            );

            Assert.Equal(
                8.0,
                result.Ingredients[0].MaximumPerSecond,
                6
            );

            Assert.Equal(
                24.0,
                result.Ingredients[1].MaximumPerSecond,
                6
            );
        }

        [Fact]
        public void Calculate_IncompatibleFactory_ThrowsException()
        {
            // Arrange
            Recipe recipe = CreateElectronicCircuitRecipe();

            Factory factory = new Factory
            {
                Id = "crusher",
                CraftingSpeed = 1,
                SupportedCategories = new List<string>
                {
                    "crushing"
                }
            };

            ProductionCalculationService service =
                new ProductionCalculationService();

            // Act
            Action action = delegate
            {
                service.Calculate(
                    recipe,
                    factory,
                    "electronic-circuit",
                    10
                );
            };

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void Calculate_ZeroTarget_ThrowsException()
        {
            // Arrange
            Recipe recipe = CreateElectronicCircuitRecipe();
            Factory factory = CreateElectromagneticPlant();

            ProductionCalculationService service =
                new ProductionCalculationService();

            // Act
            Action action = delegate
            {
                service.Calculate(
                    recipe,
                    factory,
                    "electronic-circuit",
                    0
                );
            };

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(action);
        }

        private static Recipe CreateElectronicCircuitRecipe()
        {
            Recipe recipe = new Recipe
            {
                Id = "electronic-circuit",
                CraftTime = 0.5,
                Categories = new List<string>
                {
                    "crafting",
                    "electromagnetics"
                },
                AllowProductivity = true
            };

            recipe.Ingredients.Add(new RecipeItem
            {
                ItemId = "iron-plate",
                Amount = 1
            });

            recipe.Ingredients.Add(new RecipeItem
            {
                ItemId = "copper-cable",
                Amount = 3
            });

            recipe.Products.Add(new RecipeItem
            {
                ItemId = "electronic-circuit",
                Amount = 1
            });

            return recipe;
        }

        [Fact]
        public void Calculate_AlternativeRecipe_UsesTargetProduct()
        {
            // Arrange
            Recipe recipe = new Recipe
            {
                Id = "casting-iron",
                CraftTime = 3.2,
                Categories = new List<string>
        {
            "metallurgy"
        }
            };

            recipe.Products.Add(new RecipeItem
            {
                ItemId = "iron-plate",
                Amount = 2
            });

            Factory factory = new Factory
            {
                Id = "foundry",
                CraftingSpeed = 4,
                BaseProductivity = 0.5,
                SupportedCategories = new List<string>
        {
            "metallurgy"
        }
            };

            Mock<IFactorioDataService> mock =
                new Mock<IFactorioDataService>();

            mock.Setup(service => service.GetRecipeById("casting-iron"))
                .Returns(recipe);

            mock.Setup(service => service.GetFactoryById("foundry"))
                .Returns(factory);

            ProductionBlock block = new ProductionBlock();

            block.Steps.Add(new ProductionStep
            {
                RecipeId = "casting-iron",
                TargetProductId = "iron-plate",
                FactoryId = "foundry",
                TargetProductionPerSecond = 10
            });

            ProductionBlockCalculationService service =
                new ProductionBlockCalculationService(
                    mock.Object,
                    new ProductionCalculationService()
                );

            // Act
            var result = service.Calculate(block);

            // Assert
            Assert.Single(result.Steps);
            Assert.Equal(
                "iron-plate",
                result.Steps[0].Calculation.TargetProductId
            );
            Assert.Equal(
                3.75,
                result.Steps[0].Calculation.ProductionPerFactory,
                6
            );
            Assert.Equal(3, result.Steps[0].Calculation.FactoryCount);
        }

        private static Factory CreateElectromagneticPlant()
        {
            return new Factory
            {
                Id = "electromagnetic-plant",
                CraftingSpeed = 2,
                ModuleSlots = 5,
                BaseProductivity = 0.5,
                SupportedCategories = new List<string>
                {
                    "electromagnetics"
                }
            };
        }
    }
}
