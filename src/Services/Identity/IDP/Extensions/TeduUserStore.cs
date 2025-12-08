using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.Persistence;

namespace IDP.Extensions;

public class TeduUserStore : UserStore<User, IdentityRole, TeduIdentityContext>
{
    public TeduUserStore(TeduIdentityContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
    }

    public override async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = new CancellationToken())
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        var userId = user.Id;
        var query = from userRole in Context.Set<IdentityUserRole<string>>()
                    join role in Context.Set<IdentityRole>() on userRole.RoleId equals role.Id
                    where userRole.UserId == userId
                    select role.Name;
        
        var roles = await query.ToListAsync(cancellationToken);
        return roles.Where(r => r != null).Cast<string>().ToList();
    }
}
