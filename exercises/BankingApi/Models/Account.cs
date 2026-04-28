using System.Text.Json.Serialization;

namespace BankingApi.Models;

public class Account : IHasUpdatedAt
{
    public int Id { get; set; }

    public string AccountNumber { get; set; } = string.Empty;

    public List<Transaction>? Transactions { get; set; }

    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    [JsonIgnore] public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}