
using System.Text.Json;
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Domain.Models;

namespace FactorioPlanner.Infrastructure.Services
{
    public class FactorioDataService : IFactorioDataService
    {
        private readonly List<Recipe> recipes;
        private readonly List<Factory> factories;

        public FactorioDataService(string dataDirectory)
        {
            recipes = LoadRecipes(
                Path.Combine(dataDirectory, "recipes.json")
            );

            factories = LoadFactories(
                Path.Combine(dataDirectory, "factories.json")
            );
        }

        public List<Recipe> GetRecipes()
        {
            return new List<Recipe>(recipes);
        }


        public List<Recipe> GetRecipesProducingItem(string itemId)
        {
            List<Recipe> result = new List<Recipe>();

            if (string.IsNullOrWhiteSpace(itemId))
            {
                return result;
            }

            foreach (Recipe recipe in recipes)
            {
                foreach (RecipeItem product in recipe.Products)
                {
                    if (string.Equals(
                        product.ItemId,
                        itemId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(recipe);
                        break;
                    }
                }
            }

            return result;
        }


        public Recipe? GetRecipeById(string id)
        {
            foreach (Recipe recipe in recipes)
            {
                if (recipe.Id == id)
                {
                    return recipe;
                }
            }

            return null;
        }

        public List<Factory> GetFactories()
        {
            return new List<Factory>(factories);
        }

        public Factory? GetFactoryById(string id)
        {
            foreach (Factory factory in factories)
            {
                if (factory.Id == id)
                {
                    return factory;
                }
            }

            return null;
        }

        private List<Recipe> LoadRecipes(string filePath)
        {
            using JsonDocument document = JsonDocument.Parse(
                File.ReadAllText(filePath)
            );

            List<Recipe> result = new List<Recipe>();

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                string id = element.GetProperty("Id").GetString() ?? "";

                Recipe recipe = new Recipe
                {
                    Id = id,
                    Name = element.GetProperty("Name").GetString() ?? id,
                    CraftTime = element.GetProperty("CraftTime").GetDouble()
                };

                if (element.TryGetProperty("AllowProductivity", out JsonElement allow))
                {
                    recipe.AllowProductivity = allow.ValueKind == JsonValueKind.True;
                }

                if (element.TryGetProperty("Categories", out JsonElement categories) &&
                    categories.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement category in categories.EnumerateArray())
                    {
                        string? value = category.GetString();

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            recipe.Categories.Add(value);
                        }
                    }
                }

                if (recipe.Categories.Count == 0 &&
                    element.TryGetProperty("Category", out JsonElement singleCategory))
                {
                    string? category = singleCategory.GetString();

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        recipe.Categories.Add(category);
                    }
                }


                if (element.TryGetProperty("Ingredients", out JsonElement ingredients) &&
                    ingredients.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement ingredient in ingredients.EnumerateArray())
                    {
                        string itemId = string.Empty;

                        if (ingredient.TryGetProperty("ItemId", out JsonElement itemIdElement))
                        {
                            itemId = itemIdElement.GetString() ?? string.Empty;
                        }
                        else if (ingredient.TryGetProperty("Name", out JsonElement nameElement))
                        {
                            itemId = nameElement.GetString() ?? string.Empty;
                        }

                        double amount = ingredient.GetProperty("Amount").GetDouble();

                        bool isFluid =
                            ingredient.TryGetProperty("IsFluid", out JsonElement fluidElement) &&
                            fluidElement.ValueKind == JsonValueKind.True;

                        recipe.Ingredients.Add(new RecipeItem
                        {
                            ItemId = itemId,
                            Amount = amount,
                            IsFluid = isFluid
                        });
                    }
                }

                if (element.TryGetProperty("Products", out JsonElement products) &&
                    products.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement product in products.EnumerateArray())
                    {
                        recipe.Products.Add(new RecipeItem
                        {
                            ItemId = product.GetProperty("ItemId").GetString() ?? "",
                            Amount = product.GetProperty("Amount").GetDouble(),
                            IsFluid =
                                product.TryGetProperty("IsFluid", out JsonElement fluidElement) &&
                                fluidElement.ValueKind == JsonValueKind.True
                        });
                    }
                }
                else
                {
                    recipe.Products.Add(new RecipeItem
                    {
                        ItemId = id,
                        Amount = element.GetProperty("OutputAmount").GetDouble()
                    });
                }


                recipe.Products.Add(new RecipeItem
                {
                    ItemId = id,
                    Amount = element.GetProperty("OutputAmount").GetDouble()
                });

                result.Add(recipe);
            }

            return result;
        }

        private List<Factory> LoadFactories(string filePath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<Factory>? result = JsonSerializer.Deserialize<List<Factory>>(
                File.ReadAllText(filePath),
                options
            );

            return result ?? new List<Factory>();
        }
    }
}
