
namespace FactorioPlanner.Domain.Models
{
    public class ProductionPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ProductionBlock> Blocks { get; set; } = new List<ProductionBlock>();
    }
}
