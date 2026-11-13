
namespace FactorioPlanner.Application.Models
{
    public class ProductionBlockResult
    {
        public Guid BlockId
        {
            get; set;
        }

        public string BlockName { get; set; } = string.Empty;

        public List<ProductionStepResult> Steps
        {
            get; set;
        } =
            new List<ProductionStepResult>();

        public int TotalFactoryCount
        {
            get; set;
        }
    }
}
