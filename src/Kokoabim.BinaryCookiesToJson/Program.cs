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

        new ConsoleArgument("domain", identifier: "d", helpText: "Filter cookies by domain", type: ArgumentType.Option),
        new ConsoleArgument("name", identifier: "n", helpText: "Filter cookies by name", type: ArgumentType.Option),

        new ConsoleArgument("keyvalue", identifier: "k", helpText: "Output key=value pairs", type: ArgumentType.Switch),
        new ConsoleArgument("value", identifier: "v", helpText: "Output values only", type: ArgumentType.Switch),

        new ConsoleArgument("join", identifier: "j", helpText: "Join output keyvalue pairs or values", type: ArgumentType.Switch),
        new ConsoleArgument("no-newline", identifier: "n", helpText: "Output without newline at end", type: ArgumentType.Switch),
    ],
    titleText: "Binary cookies file to JSON file converter",
    asyncFunction: async context =>
    {
        var cookies = BinaryCookiesFileParser.ParseFile(context.GetString("input"));
        if (context.GetOptionValueOrDefault("domain") is string domain && !string.IsNullOrEmpty(domain)) cookies = cookies.Where(cookie => cookie.Domain.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
        if (context.GetOptionValueOrDefault("name") is string name && !string.IsNullOrEmpty(name)) cookies = cookies.Where(cookie => cookie.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (!cookies.Any())
        {
            Console.Error.WriteLine("No cookies found matching the specified criteria.");
            return 1;
        }

        var joinDelimiter = context.HasSwitch("join") ? "; " : "\n";

        var output = context.HasSwitch("join") || context.HasSwitch("keyvalue") || context.HasSwitch("value")
            ? cookies.Select(cookie => context.HasSwitch("value") ? $"{cookie.Value}" : $"{cookie.Name}={cookie.Value}").Aggregate((current, next) => $"{current}{joinDelimiter}{next}")
            : JsonSerializer.Serialize(cookies, jsonOptions);

        if (context.GetStringOrDefault("output") is not string outputPath)
        {
            if (context.HasSwitch("no-newline")) Console.Write(output);
            else Console.WriteLine(output);
        }
        else if (File.Exists(outputPath) && !context.HasSwitch("overwrite"))
        {
            Console.Error.WriteLine("Output file already exists. (use -o to overwrite)");
            return 1;
        }
        else
        {
            await File.WriteAllTextAsync(outputPath, output);
            Console.WriteLine($"Cookies written to {outputPath}");
        }

        return 0;
    }
);

return await consoleApp.RunAsync(args);
