namespace BankingApi.Dtos;

using BankingApi.Models;

public record AccountResponse(
    int Id,
    string AccountNumber,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    decimal? Balance,
    List<TransactionResponse>? Transactions,
    int? BranchId
)
{
    public static AccountResponse FromEntity(Account a)
    {
        decimal? balance = a.Transactions == null
            ? null
            : a.Transactions.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Amount)
              - a.Transactions.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Amount);

        return new AccountResponse(
            a.Id,
            a.AccountNumber,
            a.CreatedAt,
            a.UpdatedAt,
            balance,
            a.Transactions?.Select(TransactionResponse.FromEntity).ToList(),
            a.BranchId
        );
    }
}