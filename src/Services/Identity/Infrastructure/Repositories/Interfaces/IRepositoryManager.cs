using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using IDP.Infrastructure.Entities;

namespace IDP.Infrastructure.Repositories.Interfaces;

public interface IRepositoryManager
{
    UserManager<User> UserManager { get; }
    RoleManager<IdentityRole> RoleManager { get; }
    IPermissionRepository Permission { get; }
    Task<int> SaveAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task EndTransactionAsync();
    void RollbackTransaction();
}
