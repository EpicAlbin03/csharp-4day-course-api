using System.ComponentModel.DataAnnotations;

namespace BankingApi.Dtos;

public record LoginRequest(
    [Required] [EmailAddress] string Email,
    [Required] string Password
);