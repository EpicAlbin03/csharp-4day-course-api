namespace BankingApi.Models;

public interface IHasUpdatedAt
{
    DateTime? UpdatedAt { get; set; }
}