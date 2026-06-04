using System.Text.Json;

namespace HotPathTrace.Core;

public static class NdjsonTradingEventReader
{
    public static IReadOnlyList<TradingEvent> Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var events = new List<TradingEvent>();
        var lineNumber = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                events.Add(JsonSerializer.Deserialize<TradingEvent>(line));
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    $"Line {lineNumber} could not be parsed as a TradingEvent JSON object.",
                    exception);
            }
        }

        return events;
    }
}
