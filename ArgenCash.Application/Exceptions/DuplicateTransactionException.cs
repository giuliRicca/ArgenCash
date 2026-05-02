namespace ArgenCash.Application.Exceptions;

public class DuplicateTransactionException : Exception
{
    public DuplicateTransactionException(string message) : base(message)
    {
    }
}
