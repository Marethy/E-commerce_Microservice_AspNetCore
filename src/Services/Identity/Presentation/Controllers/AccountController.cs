using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.ViewModels;
using Contracts.Identity;
using Shared.DTOs.Identity;
using IDP.Infrastructure.Repositories.Interfaces;
using Shared.Common.Helpers;

namespace IDP.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize("Bearer")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRepositoryManager _repositoryManager;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IRepositoryManager repositoryManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _repositoryManager = repositoryManager;
    }

    [HttpGet]
    public async Task<IActionResult> UserInfo()
    {
        var user = await _userManager.FindByNameAsync(User.Identity!.Name);
        return Ok(user);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new AuthResponse { IsSuccess = false, Message = "User not authenticated" });

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound(new AuthResponse { IsSuccess = false, Message = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);

        var response = new AuthResponse
        {
            IsSuccess = true,
            Message = "User info retrieved successfully",
            Username = user.UserName,
            Email = user.Email,
            Roles = roles.ToList()
        };

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new User
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Administrator");

        var response = new AuthResponse
        {
            IsSuccess = true,
            Message = "User registered successfully with Administrator role",
            Username = user.UserName,
            Email = user.Email
        };

        return CreatedAtAction(nameof(UserInfo), null, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized(new AuthResponse { IsSuccess = false, Message = "Invalid username or password" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new AuthResponse { IsSuccess = false, Message = "Invalid username or password" });

        var roles = await _userManager.GetRolesAsync(user);
        var userPermissions = await _repositoryManager.Permission.GetPermissionsByUser(user);
        var permissions = userPermissions.Select(p => $"{p.Function}.{p.Command}").Distinct().ToList();

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Permissions = permissions
        };

        var tokenResponse = _tokenService.GenerateToken(userInfo);

        var response = new AuthResponse
        {
            IsSuccess = true,
            Message = "Login successful",
            Token = tokenResponse.Token,
            ExpiresIn = tokenResponse.ExpiresIn,
            Username = user.UserName,
            Email = user.Email,
            Roles = roles.ToList()
        };

        return Ok(response);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized(new AuthResponse { IsSuccess = false, Message = "User not authenticated" });

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound(new AuthResponse { IsSuccess = false, Message = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new AuthResponse
            {
                IsSuccess = false,
                Message = "Failed to change password",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });

        return Ok(new AuthResponse { IsSuccess = true, Message = "Password changed successfully" });
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
    public IActionResult Logout()
    {
        // JWT tokens are stateless - client should simply discard the token
        // For Duende IdentityServer users, they should call /connect/revocation
        return Ok(new AuthResponse { IsSuccess = true, Message = "Logged out successfully" });
    }
}
