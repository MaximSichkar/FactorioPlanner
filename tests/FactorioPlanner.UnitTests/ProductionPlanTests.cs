
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.UnitTests
{
    public class ProductionPlanTests
    {
        [Fact]
        public void NewProductionPlan_ShouldHaveEmptyBlocks()
        {
            // Arrange
            string planName = "Electronic Circuit Factory";

            // Act
            ProductionPlan plan = new ProductionPlan
            {
                Name = planName
            };

            // Assert
            Assert.Equal(planName, plan.Name);
            Assert.NotEqual(Guid.Empty, plan.Id);
            Assert.Empty(plan.Blocks);
        }

        [Fact]
        public void ProductionPlan_ShouldAllowAddingBlock()
        {
            // Arrange
            ProductionPlan plan = new ProductionPlan();

            ProductionBlock block = new ProductionBlock
            {
                Name = "Electronic Circuits"
            };

            // Act
            plan.Blocks.Add(block);

            // Assert
            Assert.Single(plan.Blocks);
            Assert.Equal("Electronic Circuits", plan.Blocks[0].Name);
        }

        [Fact]
        public void ProductionBlock_ShouldAllowAddingStep()
        {
            // Arrange
            ProductionBlock block = new ProductionBlock();

            ProductionStep step = new ProductionStep
            {
                RecipeId = "electronic-circuit",
                FactoryId = "electromagnetic-plant",
                TargetProductionPerSecond = 10
            };

            // Act
            block.Steps.Add(step);

            // Assert
            Assert.Single(block.Steps);
            Assert.Equal("electronic-circuit", block.Steps[0].RecipeId);
            Assert.Equal(10, block.Steps[0].TargetProductionPerSecond);
        }
    }
}
