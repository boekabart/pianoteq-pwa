namespace Pianoteq.Client.Exceptions;

public class PianoteqException : Exception
{
    public int? ErrorCode { get; }
    public object? ErrorData { get; }

    public PianoteqException(string message) : base(message)
    {
    }

    public PianoteqException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public PianoteqException(string message, int errorCode, object? errorData = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorData = errorData;
    }
}
