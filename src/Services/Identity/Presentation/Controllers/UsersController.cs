using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.ViewModels;

namespace IDP.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize("Bearer")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserViewModel>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUsers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _userManager.Users
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userViewModels = new List<UserViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        return Ok(userViewModels);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        var roles = await _userManager.GetRolesAsync(user);
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };

        return Ok(userViewModel);
    }

    /// <summary>
    /// Create new user with specific role (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate role exists
        if (!await _roleManager.RoleExistsAsync(model.Role))
            return BadRequest($"Role '{model.Role}' does not exist");

        var user = new User
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName ?? string.Empty,
            LastName = model.LastName ?? string.Empty
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Assign role
        await _userManager.AddToRoleAsync(user, model.Role);

        var roles = await _userManager.GetRolesAsync(user);
        var userViewModel = new UserViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userViewModel);
    }

    /// <summary>
    /// Update user information (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        user.FirstName = model.FirstName ?? user.FirstName;
        user.LastName = model.LastName ?? user.LastName;
        user.Email = model.Email ?? user.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Assign or change user role (Admin only)
    /// </summary>
    [HttpPost("{id}/roles")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        // Validate role exists
        if (!await _roleManager.RoleExistsAsync(model.Role))
            return BadRequest($"Role '{model.Role}' does not exist");

        // Remove existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest(removeResult.Errors);
        }

        // Add new role
        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Reset password (Admin only - no current password required)
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found");

        // Remove current password
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
            return BadRequest(removePasswordResult.Errors);

        // Set new password
        var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}
