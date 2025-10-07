using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Identity
{
    public interface  ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        IEnumerable<string> Roles { get; }
    }
}
