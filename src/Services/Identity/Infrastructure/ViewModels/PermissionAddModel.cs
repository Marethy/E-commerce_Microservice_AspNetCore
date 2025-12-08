using System.ComponentModel.DataAnnotations;

namespace IDP.Infrastructure.ViewModels;
public class PermissionAddModel
{
    [Required]
    public required string Function { get; set; }

    [Required]
    public required string Command { get; set; }
}
