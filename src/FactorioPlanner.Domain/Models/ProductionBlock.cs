
namespace FactorioPlanner.Domain.Models
{
    public class ProductionBlock
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public int Order
        {
            get; set;
        }

        public List<ProductionStep> Steps { get; set; } = new List<ProductionStep>();
    }
}
