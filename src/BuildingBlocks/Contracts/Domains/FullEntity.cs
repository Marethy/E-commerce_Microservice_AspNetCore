using Contracts.Domains.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domains
{
    public abstract class FullEntity<T> : EntityBase<T>, IFullEntity
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset DeletedDate { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
