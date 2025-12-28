namespace IDP.Infrastructure.ViewModels;

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public int? ExpiresIn { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
    public List<string>? Permissions { get; set; }
    public List<string>? Errors { get; set; }
}
