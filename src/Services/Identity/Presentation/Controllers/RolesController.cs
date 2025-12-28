using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace IDP.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize("Bearer")]
public class RolesController(RoleManager<IdentityRole> roleManager) : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IdentityRole>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IdentityRole), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound($"Role with ID {id} not found");

        return Ok(role);
    }

    /// <summary>
    /// Create new role
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IdentityRole), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = new IdentityRole(model.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
    }

    /// <summary>
    /// Update role
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound($"Role with ID {id} not found");

        role.Name = model.Name;
        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Delete role
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(IdentityError[]), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound($"Role with ID {id} not found");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}

public class CreateRoleModel
{
    public required string Name { get; set; }
}

public class UpdateRoleModel
{
    public required string Name { get; set; }
}
