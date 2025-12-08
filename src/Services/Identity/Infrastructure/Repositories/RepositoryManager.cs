using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using IDP.Infrastructure.Domains.Interfaces;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.Persistence;
using IDP.Infrastructure.Repositories.Interfaces;

namespace IDP.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TeduIdentityContext _dbContext;
    private readonly Lazy<IPermissionRepository> _permissionRepository;
    private readonly IMapper _mapper;

    public RepositoryManager(IUnitOfWork unitOfWork,
                             TeduIdentityContext dbContext,
                             UserManager<User> userManager,
                             RoleManager<IdentityRole> roleManager,
                             IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        UserManager = userManager;
        RoleManager = roleManager;
        _mapper = mapper;
        _permissionRepository = new Lazy<IPermissionRepository>(() => new PermissionRepository(_dbContext, _unitOfWork, UserManager, RoleManager, _mapper));
    }

    public UserManager<User> UserManager { get; }
    public RoleManager<IdentityRole> RoleManager { get; }
    public IPermissionRepository Permission => _permissionRepository.Value;

    public Task<int> SaveAsync() => _unitOfWork.CommitAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync() => _dbContext.Database.BeginTransactionAsync();

    public Task EndTransactionAsync() => _dbContext.Database.CommitTransactionAsync();

    public void RollbackTransaction() => _dbContext.Database.RollbackTransactionAsync();
}
