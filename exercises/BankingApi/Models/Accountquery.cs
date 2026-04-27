namespace BankingApi.Models;

public class AccountQuery
{
    public string? Include { get; set; }
    public TransactionType? Type { get; set; }
}