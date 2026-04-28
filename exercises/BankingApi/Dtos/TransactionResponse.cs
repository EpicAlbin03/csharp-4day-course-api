namespace BankingApi.Dtos;

using BankingApi.Models;

public record TransactionResponse(
    int Id,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime Timestamp,
    int AccountId
)
{
    public static TransactionResponse FromEntity(Transaction t) =>
        new(t.Id, t.Type, t.Amount, t.Description, t.Timestamp, t.AccountId);
}