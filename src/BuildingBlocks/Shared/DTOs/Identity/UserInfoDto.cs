namespace Shared.DTOs.Identity;

public class UserInfoDto
{
 public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}
