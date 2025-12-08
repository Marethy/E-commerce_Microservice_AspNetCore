using System.ComponentModel.DataAnnotations;

namespace IDP.Infrastructure.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateUserModel
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    [Required]
    public string Role { get; set; } = default!; // Administrator, Customer, Agent

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class UpdateUserModel
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class AssignRoleModel
{
    [Required]
    public string Role { get; set; } = default!;
}

public class ChangePasswordModel
{
    [Required]
    public string CurrentPassword { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = default!;
}

public class ResetPasswordModel
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = default!;
}
