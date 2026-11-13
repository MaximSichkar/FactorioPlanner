
namespace FactorioPlanner.Application.Models
{
    public class ResourceBalance
    {
        public string ItemId { get; set; } = string.Empty;

        public bool IsFluid
        {
            get; set;
        }

        public double RequiredPerSecond
        {
            get; set;
        }

        public double ProducedPerSecond
        {
            get; set;
        }

        public double ExternalRequiredPerSecond
        {
            get; set;
        }

        public double SurplusPerSecond
        {
            get; set;
        }
    }
}
