namespace TransactionApi.Domain.Exceptions;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(string accountId)
        : base($"Account '{accountId}' was not found.") { }
}
