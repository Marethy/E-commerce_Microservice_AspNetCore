using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domains.Interfaces
{
    public interface ISoftDeletable
    {

        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset DeletedDate { get; set; }


    }
}
