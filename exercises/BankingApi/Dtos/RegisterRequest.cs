using System.ComponentModel.DataAnnotations;

namespace BankingApi.Dtos;

public record RegisterRequest(
    [Required] [EmailAddress] string Email,
    [Required]
    [StringLength(100, MinimumLength = 6)]
    string Password
);