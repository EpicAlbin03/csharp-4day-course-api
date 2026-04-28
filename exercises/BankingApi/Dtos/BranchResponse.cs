namespace BankingApi.Dtos;

using BankingApi.Models;

public record BranchResponse(
    int Id,
    string Name,
    string? Address,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public static BranchResponse FromEntity(Branch b) =>
        new(b.Id, b.Name, b.Address, b.CreatedAt, b.UpdatedAt);
}