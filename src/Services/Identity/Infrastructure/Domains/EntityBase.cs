using IDP.Infrastructure.Domains.Interfaces;

namespace IDP.Infrastructure.Domains;

public abstract class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}
