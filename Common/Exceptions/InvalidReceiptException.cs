namespace Common.Exceptions;

public class InvalidReceiptException: Exception
{
    public InvalidReceiptException()
    {
    }

    public InvalidReceiptException(string message)
        : base(message)
    {
    }

    public InvalidReceiptException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}