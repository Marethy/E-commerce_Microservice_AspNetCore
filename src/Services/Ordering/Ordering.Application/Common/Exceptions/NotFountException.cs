using System;

namespace Ordering.Application.Common.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException() : base()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string entity, object key)
            : base($"Entity \"{entity}\" ({key}) was not found.")
        {
        }
    }
}


