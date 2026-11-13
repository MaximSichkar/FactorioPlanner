
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FactorioPlanner.Infrastructure.Importers
{
    public class RecipeDataImporter
    {
        public int Import(
            string existingRecipesPath,
            string rawDumpPath,
            string outputPath)
        {
            JsonArray recipes = JsonNode.Parse(
                File.ReadAllText(existingRecipesPath)
            ) as JsonArray
                ?? throw new InvalidDataException(
                    "Неверный формат recipes.json."
                );

            JsonObject dataRoot = JsonNode.Parse(
                File.ReadAllText(rawDumpPath)
            ) as JsonObject
                ?? throw new InvalidDataException(
                    "Неверный формат data-raw-dump.json."
                );

            JsonObject rawRecipes = dataRoot["recipe"] as JsonObject
                ?? throw new InvalidDataException(
                    "В дампе отсутствуют рецепты."
                );

            int updatedCount = 0;

            foreach (JsonNode? node in recipes)
            {
                if (node is not JsonObject recipe)
                {
                    continue;
                }

                string id = recipe["Id"]?.GetValue<string>()
                    ?? string.Empty;

                if (rawRecipes[id] is not JsonObject rawRecipe)
                {
                    continue;
                }

                if (rawRecipe["results"] is not JsonArray results)
                {
                    continue;
                }

                JsonArray products = ConvertItems(results);

                if (products.Count == 0)
                {
                    continue;
                }

                recipe["Products"] = products;

                if (rawRecipe["ingredients"] is JsonArray ingredients)
                {
                    recipe["Ingredients"] = ConvertItems(ingredients);
                }

                updatedCount++;
            }

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string outputJson = recipes.ToJsonString(options);

            File.WriteAllText(outputPath, outputJson);

            return updatedCount;
        }

        private static JsonArray ConvertItems(JsonArray source)
        {
            JsonArray result = new JsonArray();

            foreach (JsonNode? node in source)
            {
                if (node is not JsonObject item)
                {
                    continue;
                }

                string itemId = item["name"]?.GetValue<string>()
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(itemId))
                {
                    continue;
                }

                double amount = ReadNumber(item, "amount", 0);

                if (amount <= 0)
                {
                    double minimum = ReadNumber(item, "amount_min", 0);
                    double maximum = ReadNumber(item, "amount_max", 0);

                    amount = (minimum + maximum) / 2.0;
                }

                double probability = ReadNumber(
                    item,
                    "probability",
                    1.0
                );

                amount *= probability;

                if (!double.IsFinite(amount) || amount <= 0)
                {
                    continue;
                }

                bool isFluid =
                    item["type"]?.GetValue<string>() == "fluid";

                JsonObject converted = new JsonObject
                {
                    ["ItemId"] = itemId,
                    ["Amount"] = amount,
                    ["IsFluid"] = isFluid
                };

                result.Add(converted);
            }

            return result;
        }

        private static double ReadNumber(
            JsonObject item,
            string propertyName,
            double defaultValue)
        {
            JsonNode? value = item[propertyName];

            if (value == null)
            {
                return defaultValue;
            }

            return value.GetValue<double>();
        }
    }
}
