using System.Text;

namespace HotPathTrace.Core;

public static class BinaryLogFormat
{
    public static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes("HPTL");

    public const int CurrentVersion = 1;
    public const int HeaderSize = 24;
    public const int RecordSize = 37;
}
