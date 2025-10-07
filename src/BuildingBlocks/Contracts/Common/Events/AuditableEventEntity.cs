using Contracts.Domains.Interfaces;

namespace Contracts.Common.Events;

public class AuditableEventEntity<T> : EventEntity<T>, IAuditable
{
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}