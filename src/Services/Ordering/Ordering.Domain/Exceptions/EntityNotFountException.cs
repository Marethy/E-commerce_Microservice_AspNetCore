using System;

namespace Ordering.Domain.Exceptions
{
    internal class EntityNotFoundException : ApplicationException
    {
        public EntityNotFoundException(string entity, object key)
            : base($"Entity \"{entity}\" ({key}) was not found.")
        {
        }
    }
}

