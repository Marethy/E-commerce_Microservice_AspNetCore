using Contracts.Common.Events;
using Ordering.Domain.Enums;
using Ordering.Domain.OrderAggregate.Events;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ordering.Domain.Entities
{
    public class Order : AuditableEventEntity<long>
    {
        [Required]
        public string UserName { get; set; }

        public Guid DocumentNo { get; set; } = Guid.NewGuid();

        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than zero.")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string ShippingAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string InvoiceAddress { get; set; }

        public EOrderStatus Status { get; set; }

        [NotMapped]
        public string FullName => FirstName + " " + LastName;

        public Order AddedOrder()
        {
            AddDomainEvent(new OrderCreatedEvent(Id, UserName, TotalPrice, DocumentNo.ToString(), EmailAddress, ShippingAddress, InvoiceAddress, FullName));
            return this;
        }

        public Order DeletedOrder()
        {
            AddDomainEvent(new OrderDeletedEvent(Id));
            return this;
        }
    }
}