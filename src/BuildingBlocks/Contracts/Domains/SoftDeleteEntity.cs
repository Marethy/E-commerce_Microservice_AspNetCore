using Contracts.Domains.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domains
{
    public  class SoftDeleteEntity<T> : EntityBase<T>, ISoftDeletable
    {
        public bool IsDeleted { get; set; } = false;
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset DeletedDate { get; set; }
    }
}
