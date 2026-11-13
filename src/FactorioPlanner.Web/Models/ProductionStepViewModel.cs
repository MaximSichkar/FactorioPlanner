
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactorioPlanner.Web.Models
{
    public class ProductionStepViewModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string RecipeId { get; set; } = string.Empty;

        public string FactoryId { get; set; } = string.Empty;

        public double TargetProductionPerSecond { get; set; } = 10;
        public string TargetProductId { get; set; } = string.Empty; 

        public List<SelectListItem> Factories
        {
            get; set;
        } =
            new List<SelectListItem>();
    }
}
