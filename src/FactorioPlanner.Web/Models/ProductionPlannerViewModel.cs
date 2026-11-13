
using FactorioPlanner.Application.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactorioPlanner.Web.Models
{
    public class ProductionPlannerViewModel
    {
        public Guid BlockId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "Производственная линия 1";
        public string TargetProductId { get; set; } = string.Empty;

        public bool ShowRecipeChoice
        {
            get; set;
        }

        public string PendingIngredientItemId { get; set; } = string.Empty;

        public double PendingIngredientRate
        {
            get; set;
        }

        public int PendingRowIndex { get; set; } = -1;

        public List<SelectListItem> AlternativeRecipes
        {
            get; set;
        } =
            new List<SelectListItem>();


        public List<ProductionStepViewModel> Steps
        {
            get; set;
        } =
            new List<ProductionStepViewModel>();

        public List<SelectListItem> Recipes
        {
            get; set;
        } =
            new List<SelectListItem>();

        public ProductionBlockResult? Result
        {
            get; set;
        }

        public string? ErrorMessage
        {
            get; set;
        }
    }
}
