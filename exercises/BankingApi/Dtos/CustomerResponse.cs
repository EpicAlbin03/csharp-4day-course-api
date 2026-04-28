namespace BankingApi.Dtos;

using BankingApi.Models;

public record CustomerResponse(
    int Id,
    string FullName,
    string? Email,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public static CustomerResponse FromEntity(Customer c) =>
        new(c.Id, c.FullName, c.Email, c.CreatedAt, c.UpdatedAt);
}