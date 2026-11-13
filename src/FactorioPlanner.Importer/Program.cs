
using FactorioPlanner.Infrastructure.Importers;

if (args.Length != 3)
{
    Console.WriteLine(
        "Использование: Importer <recipes.json> <data-raw-dump.json> <output.json>"
    );

    return;
}

try
{
    RecipeDataImporter importer = new RecipeDataImporter();

    int count = importer.Import(
        args[0],
        args[1],
        args[2]
    );

    Console.WriteLine("Обновлено рецептов: " + count);
    Console.WriteLine("Результат: " + args[2]);
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}
    