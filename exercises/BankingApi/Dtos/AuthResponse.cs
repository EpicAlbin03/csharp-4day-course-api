using System.ComponentModel.DataAnnotations;

namespace BankingApi.Dtos;

public record AuthResponse(string AccessToken, DateTime ExpiresAt);