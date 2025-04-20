namespace Ordering.Domain.Exceptions
{
    internal class InvalidEntityTypeException(string entity, object key) : ApplicationException($"Entity \"{entity}\" ({key}) was invalid.")
    {
    }
}