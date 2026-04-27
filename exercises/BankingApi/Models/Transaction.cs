namespace BankingApi.Models;

using System.Text.Json.Serialization;

public class Transaction
{
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int AccountId { get; set; }

    [JsonIgnore] public Account? Account { get; set; }
}