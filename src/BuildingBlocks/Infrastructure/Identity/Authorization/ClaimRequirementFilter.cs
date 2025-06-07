using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Common.Constants;
using Shared.Common.Helpers;
using System.Text.Json;

namespace Infrastructure.Identity.Authorization;

public class ClaimRequirementFilter : IAuthorizationFilter
{
    private readonly CommandCode _commandCode;
    private readonly FunctionCode _functionCode;

    public ClaimRequirementFilter(CommandCode commandCode, FunctionCode functionCode)
    {
        _commandCode = commandCode;
        _functionCode = functionCode;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var permissionsClaim = context.HttpContext.User.Claims
            .SingleOrDefault(c => c.Type.Equals(SystemConstants.Claims.Permissions));

        if (permissionsClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim.Value);
        if (permissions == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!permissions.Contains(PermissionHelper.GetPermission(_functionCode, _commandCode)))
        {
            context.Result = new ForbidResult();
        }
    }
}