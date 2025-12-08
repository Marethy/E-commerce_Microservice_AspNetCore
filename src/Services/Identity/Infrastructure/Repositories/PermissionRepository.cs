using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;
using IDP.Infrastructure.Domains;
using IDP.Infrastructure.Domains.Interfaces;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.Persistence;
using IDP.Infrastructure.Repositories.Interfaces;
using IDP.Infrastructure.ViewModels;

namespace IDP.Infrastructure.Repositories;

public class PermissionRepository : RepositoryBase<Permission, long>, IPermissionRepository
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public PermissionRepository(TeduIdentityContext dbContext,
                                IUnitOfWork unitOfWork,
                                UserManager<User> userManager,
                                RoleManager<IdentityRole> roleManager,
                                IMapper mapper) : base(dbContext, unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PermissionViewModel>> GetPermissionsByRole(string roleId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@roleId", roleId);
        var result = await QueryAsync<PermissionViewModel>("Get_Permission_By_RoleId", parameters);
        return result;
    }

    public async Task<PermissionViewModel> CreatePermission(string roleId, PermissionAddModel model)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@roleId", roleId);
        parameters.Add("@function", model.Function);
        parameters.Add("@command", model.Command);
        parameters.Add("@newId", dbType: DbType.Int64, direction: ParameterDirection.Output);

        var result = await ExecuteAsync("Create_Permission", parameters);
        if (result <= 0) return null;
        var newId = parameters.Get<long>("@newId");

        return new PermissionViewModel
        {
            Id = newId,
            RoleId = roleId,
            Function = model.Function,
            Command = model.Command
        };
    }

    public Task DeletePermission(string roleId, string function, string command)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@roleId", roleId);
        parameters.Add("@function", function);
        parameters.Add("@command", command);

        return ExecuteAsync("Delete_Permission", parameters);
    }

    public Task UpdatePermissionsByRoleId(string roleId, IEnumerable<PermissionAddModel> permissionCollection)
    {
        var dt = new DataTable();
        dt.Columns.Add("Function", typeof(string));
        dt.Columns.Add("Command", typeof(string));
        dt.Columns.Add("RoleId", typeof(string));
        foreach (var item in permissionCollection)
        {
            dt.Rows.Add(item.Function, item.Command, roleId);
        }
        var parameters = new DynamicParameters();
        parameters.Add("@roleId", roleId, DbType.String);
        parameters.Add("@permissions", dt.AsTableValuedParameter("dbo.Permission"));
        return ExecuteAsync("Update_Permissions_By_RoleId", parameters);
    }

    public async Task<IEnumerable<PermissionUserViewModel>> GetPermissionsByUser(User user)
    {
        // Get user's role names
        var userRoleNames = await _userManager.GetRolesAsync(user);
        
        if (!userRoleNames.Any())
            return [];
        
        // Get role IDs from role names
        var roleIds = new List<string>();
        foreach (var roleName in userRoleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                roleIds.Add(role.Id);
            }
        }
        
        if (!roleIds.Any())
            return Enumerable.Empty<PermissionUserViewModel>();
        
        // Get permissions by role IDs
        var permissions = FindAll()
            .Where(p => roleIds.Contains(p.RoleId))
            .ToList();
        
        return _mapper.Map<IEnumerable<PermissionUserViewModel>>(permissions);
    }
}
