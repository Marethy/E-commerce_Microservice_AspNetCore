using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IDP.Infrastructure.Repositories.Interfaces;
using IDP.Infrastructure.ViewModels;

namespace IDP.Presentation.Controllers;
[Route("api/[controller]/roles/{roleId}")]
[ApiController]
[Authorize("Bearer")]
public class PermissionsController : ControllerBase
{
    private readonly IRepositoryManager _repository;

    public PermissionsController(IRepositoryManager repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionViewModel>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetPermissions(string roleId)
    {
        var result = await _repository.Permission.GetPermissionsByRole(roleId);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PermissionViewModel), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreatePermission(string roleId, PermissionAddModel model)
    {
        var result = await _repository.Permission.CreatePermission(roleId, model);
        return result != null ? Ok(result) : NoContent();
    }

    [HttpDelete("function/{function}/command/{command}")]
    [ProducesResponseType(typeof(NoContentResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeletePermission(string roleId, string function, string command)
    {
        await _repository.Permission.DeletePermission(roleId, function, command);
        return NoContent();
    }

    [HttpPost("update-permissions")]
    [ProducesResponseType(typeof(NoContentResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdatePermissions(string roleId, IEnumerable<PermissionAddModel> permissions)
    {
        await _repository.Permission.UpdatePermissionsByRoleId(roleId, permissions);
        return NoContent();
    }
}
