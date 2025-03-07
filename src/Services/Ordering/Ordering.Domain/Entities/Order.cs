﻿using Contracts.Domains;
using Ordering.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Domain.Entities
{
    public class Order : EntityAuditBase<long>
    {
        [Required]
        public required string UserName { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than zero.")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100)]
        public required string ShippingAddress { get; set; }

        [Required]
        [StringLength(100)]
        public required string InvoiceAddress { get; set; }
        public   EOrderStatus Status { get; set; }
    }
}
