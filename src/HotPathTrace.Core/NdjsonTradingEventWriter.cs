using System.Text;
using System.Text.Json;

namespace HotPathTrace.Core;

public static class NdjsonTradingEventWriter
{
    public static void Write(string filePath, IEnumerable<TradingEvent> events)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(events);

        using var stream = File.Create(filePath);
        using var writer = new StreamWriter(stream, Encoding.UTF8);

        foreach (var tradingEvent in events)
        {
            var json = JsonSerializer.Serialize(tradingEvent);
            writer.WriteLine(json);
        }
    }
}
