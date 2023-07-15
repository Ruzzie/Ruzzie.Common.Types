namespace Ruzzie.Common.Types.Diagnostics;

public class PanicException<TError> : Exception
{
    public TError Error { get; }

    public PanicException(TError error, string message) : base(message)
    {
        Error = error;
    }

    public PanicException(TError error, string message, Exception innerException) : base(message, innerException)
    {
        Error = error;
    }
}