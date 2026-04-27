namespace BankingApi.Models;

public class TransactionQuery
{
    public TransactionType? Type { get; set; }
    public decimal? MinAmount { get; set; }
    public DateTime? Since { get; set; }
}