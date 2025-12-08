using System;
using System.ComponentModel.DataAnnotations;

namespace IDP.Infrastructure.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; } = default!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = default!;

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
    }
}
