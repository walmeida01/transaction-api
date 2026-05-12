namespace TransactionApi.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(decimal balance, decimal requested)
        : base($"Insufficient funds. Balance: {balance}, Requested: {requested}.") { }
}
