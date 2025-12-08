using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Shared.Common.Constants;
using Shared.Common.Helpers;
using System.Text.Json;

namespace Infrastructure.Identity.Authorization;

public class ClaimRequirementFilter : IAuthorizationFilter
{
    private readonly CommandCode _commandCode;
    private readonly FunctionCode _functionCode;
    private readonly ILogger<ClaimRequirementFilter> _logger;

    public ClaimRequirementFilter(CommandCode commandCode, FunctionCode functionCode, ILogger<ClaimRequirementFilter> logger)
    {
        _commandCode = commandCode;
        _functionCode = functionCode;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var requiredPermission = PermissionHelper.GetPermission(_functionCode, _commandCode);
        _logger.LogInformation("🔐 Checking permission: {Permission}", requiredPermission);
        
        // Log all claims for debugging
        var allClaims = context.HttpContext.User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
        _logger.LogInformation("📋 All claims: {Claims}", string.Join(", ", allClaims));

        var permissionsClaim = context.HttpContext.User.Claims
            .SingleOrDefault(c => c.Type.Equals(SystemConstants.Claims.Permissions));

        if (permissionsClaim == null)
        {
            _logger.LogWarning("❌ No Permissions claim found. Available claim types: {Types}", 
                string.Join(", ", context.HttpContext.User.Claims.Select(c => c.Type)));
            context.Result = new ForbidResult();
            return;
        }

        _logger.LogInformation("✅ Permissions claim found: {Value}", permissionsClaim.Value);

        var permissions = JsonSerializer.Deserialize<List<string>>(permissionsClaim.Value);
        if (permissions == null)
        {
            _logger.LogWarning("❌ Failed to deserialize permissions");
            context.Result = new ForbidResult();
            return;
        }

        _logger.LogInformation("📜 User has {Count} permissions: {Permissions}", permissions.Count, string.Join(", ", permissions));

        if (!permissions.Contains(requiredPermission))
        {
            _logger.LogWarning("❌ Permission denied. Required: {Required}, User has: {UserPermissions}", 
                requiredPermission, string.Join(", ", permissions));
            context.Result = new ForbidResult();
        }
        else
        {
            _logger.LogInformation("✅ Permission granted: {Permission}", requiredPermission);
        }
    }
}