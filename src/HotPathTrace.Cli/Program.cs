using HotPathTrace.Core;

namespace HotPathTrace.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            return Fail("Missing command.", includeUsage: true);
        }

        try
        {
            return args[0] switch
            {
                "generate" => RunGenerate(args[1..]),
                "replay" => RunReplay(args[1..]),
                _ => Fail($"Unknown command '{args[0]}'.", includeUsage: true)
            };
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or ArgumentOutOfRangeException
            or DirectoryNotFoundException
            or FileNotFoundException
            or InvalidDataException
            or ReplayValidationException)
        {
            return Fail(exception.Message, includeUsage: false);
        }
    }

    private static int RunGenerate(string[] args)
    {
        var options = ParseOptions(args);
        var events = ParsePositiveIntOption(options, "--events");
        var outputPath = ParseRequiredOption(options, "--output");
        var seed = ParseOptionalIntOption(options, "--seed");

        EnsureNoUnexpectedOptions(options, "--events", "--output", "--seed");
        EnsureOutputDirectoryExists(outputPath);

        var generatedEvents = TradingEventGenerator.Generate(events, seed);
        NdjsonTradingEventWriter.Write(outputPath, generatedEvents);

        var summary = SessionSummaryCalculator.Calculate(generatedEvents);

        Console.WriteLine($"Generated {generatedEvents.Count} events to '{outputPath}'.");
        PrintSummary(summary);

        return 0;
    }

    private static int RunReplay(string[] args)
    {
        var options = ParseOptions(args);
        var filePath = ParseRequiredOption(options, "--file");

        EnsureNoUnexpectedOptions(options, "--file");

        var replayedEvents = NdjsonTradingEventReader.Read(filePath);
        TradingEventReplayValidator.Validate(replayedEvents);

        var summary = SessionSummaryCalculator.Calculate(replayedEvents);

        Console.WriteLine($"Replayed and validated {replayedEvents.Count} events from '{filePath}'.");
        PrintSummary(summary);

        return 0;
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var index = 0; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for option '{args[index]}'.");
            }

            var option = args[index];
            var value = args[index + 1];

            if (!option.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected argument '{option}'. Options must start with '--'.");
            }

            if (!options.TryAdd(option, value))
            {
                throw new ArgumentException($"Option '{option}' was provided more than once.");
            }
        }

        return options;
    }

    private static string ParseRequiredOption(Dictionary<string, string> options, string optionName)
    {
        if (!options.TryGetValue(optionName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Missing required option '{optionName}'.");
        }

        return value;
    }

    private static int ParsePositiveIntOption(Dictionary<string, string> options, string optionName)
    {
        var rawValue = ParseRequiredOption(options, optionName);

        if (!int.TryParse(rawValue, out var value) || value <= 0)
        {
            throw new ArgumentException($"Option '{optionName}' must be a positive integer.");
        }

        return value;
    }

    private static int? ParseOptionalIntOption(Dictionary<string, string> options, string optionName)
    {
        if (!options.TryGetValue(optionName, out var rawValue))
        {
            return null;
        }

        if (!int.TryParse(rawValue, out var value))
        {
            throw new ArgumentException($"Option '{optionName}' must be an integer.");
        }

        return value;
    }

    private static void EnsureNoUnexpectedOptions(Dictionary<string, string> options, params string[] expectedOptions)
    {
        var expected = expectedOptions.ToHashSet(StringComparer.Ordinal);

        foreach (var option in options.Keys)
        {
            if (!expected.Contains(option))
            {
                throw new ArgumentException($"Option '{option}' is not valid for this command.");
            }
        }
    }

    private static void EnsureOutputDirectoryExists(string outputPath)
    {
        var directoryPath = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        Directory.CreateDirectory(directoryPath);
    }

    private static void PrintSummary(SessionSummary summary)
    {
        Console.WriteLine("Session summary:");
        Console.WriteLine($"  Event count: {summary.EventCount}");
        Console.WriteLine($"  First sequence: {summary.FirstSequence}");
        Console.WriteLine($"  Last sequence: {summary.LastSequence}");
        Console.WriteLine($"  Checksum: 0x{summary.Checksum:X16}");
    }

    private static int Fail(string message, bool includeUsage)
    {
        Console.Error.WriteLine($"Error: {message}");

        if (includeUsage)
        {
            Console.Error.WriteLine("Usage:");
            Console.Error.WriteLine("  generate --events <positive integer> --output <file path> [--seed <integer>]");
            Console.Error.WriteLine("  replay --file <file path>");
        }

        return 1;
    }
}
