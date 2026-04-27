namespace BankingApi.Models;

using System.Text.Json.Serialization;

public class Branch : IHasUpdatedAt
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    [JsonIgnore] public List<Account>? Accounts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}