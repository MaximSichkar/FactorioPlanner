
using System.ComponentModel.DataAnnotations;
using FactorioPlanner.Application.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactorioPlanner.Web.Models
{
    public class CalculatorViewModel
    {
        [Required(ErrorMessage = "Выберите рецепт.")]
        public string RecipeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите завод.")]
        public string FactoryId { get; set; } = string.Empty;

        [Range(
            0.000001,
            double.MaxValue,
            ErrorMessage = "Производство должно быть больше нуля."
        )]
        public double TargetProductionPerSecond { get; set; } = 10;

        public List<SelectListItem> Recipes
        {
            get; set;
        } =
            new List<SelectListItem>();

        public List<SelectListItem> Factories
        {
            get; set;
        } =
            new List<SelectListItem>();

        public ProductionCalculationResult? Result
        {
            get; set;
        }

        public string? ErrorMessage
        {
            get; set;
        }
    }
}
