
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.Application.Interfaces
{
    public interface IFactorioDataService
    {
        List<Recipe> GetRecipes();

        Recipe? GetRecipeById(string id);

        List<Factory> GetFactories();

        Factory? GetFactoryById(string id);

        List<Recipe> GetRecipesProducingItem(string itemId);
    }
}
