using Contracts.Domains.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domains
{
    public class EntityBase<TKey>:IEntityBase<TKey>
    {
        public TKey Id { get; set; }
    }
}
