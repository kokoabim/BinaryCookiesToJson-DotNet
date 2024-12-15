using System.Text.Json;
using Kokoabim.BinaryCookiesToJson;
using Kokoabim.CommandLineInterface;

var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

var consoleApp = new ConsoleApp([
        new ConsoleArgument("input",
            helpText: "Path to input binary cookies file",
            isRequired: true,
            preProcesses: ArgumentPreProcesses.ExpandEnvironmentVariables,
            constraints: ArgumentConstraints.FileMustExist),

        new ConsoleArgument("output", helpText: "Path to output JSON file", preProcesses: ArgumentPreProcesses.ExpandEnvironmentVariables),

        new ConsoleArgument("overwrite", identifier: "o", helpText: "Overwrite output file if it exists", type: ArgumentType.Switch),
    ],
    titleText: "Binary cookies file to JSON file converter",
    asyncFunction: async context =>
    {
        var cookies = BinaryCookiesFileParser.ParseFile(context.Get("input").AsString());
        var json = JsonSerializer.Serialize(cookies, jsonOptions);

        if (context.Get("output").GetValueOrNull() is not string outputPath)
        {
            Console.WriteLine(JsonSerializer.Serialize(cookies, jsonOptions));
            return 0;
        }

        if (File.Exists(outputPath) && !context.GetSwitch("o").Exists)
        {
            Console.WriteLine("Output file already exists. Use -o to overwrite.");
            return 1;
        }

        await File.WriteAllTextAsync(outputPath, json);
        Console.WriteLine($"Cookies written to {outputPath}");
        return 0;
    }
);

return await consoleApp.RunAsync(args);
