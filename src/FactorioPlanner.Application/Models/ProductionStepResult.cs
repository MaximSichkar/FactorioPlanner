
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.Application.Models
{
    public class ProductionStepResult
    {
        public Guid StepId
        {
            get; set;
        }

        public int Order
        {
            get; set;
        }

        public ProductionCalculationResult Calculation { get; set; } = null!;
    }
}
