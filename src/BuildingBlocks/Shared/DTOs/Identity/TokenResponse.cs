namespace Shared.DTOs.Identity;

public record TokenResponse(string Token, int ExpiresIn = 1800);
