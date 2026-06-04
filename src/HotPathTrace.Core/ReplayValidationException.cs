namespace HotPathTrace.Core;

public sealed class ReplayValidationException : Exception
{
    public ReplayValidationException(string message)
        : base(message)
    {
    }
}
