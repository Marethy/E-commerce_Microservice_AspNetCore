using AutoMapper;
using MediatR;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using Shared.SeedWork;
using System;
using System.ComponentModel.DataAnnotations;

namespace Ordering.Application.Features.V1.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<ApiResult<long>>
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than zero.")]
        public decimal TotalPrice { get; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; }

        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(100, ErrorMessage = "Shipping address cannot exceed 100 characters.")]
        public string ShippingAddress { get; }

        [Required(ErrorMessage = "Invoice address is required.")]
        [StringLength(100, ErrorMessage = "Invoice address cannot exceed 100 characters.")]
        public string InvoiceAddress { get; }

        public EOrderStatus Status { get; }

        public CreateOrderCommand(
            string userName,
            decimal totalPrice,
            string firstName,
            string lastName,
            string email,
            string shippingAddress,
            string invoiceAddress,
            EOrderStatus status = EOrderStatus.Pending)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            TotalPrice = totalPrice >= 0.01m ? totalPrice : throw new ArgumentException("Total price must be greater than zero", nameof(totalPrice));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
            InvoiceAddress = invoiceAddress ?? throw new ArgumentNullException(nameof(invoiceAddress));
            Status = status;

            // Kiểm tra độ dài chuỗi
            if (firstName.Length > 50) throw new ArgumentException("First name cannot exceed 50 characters", nameof(firstName));
            if (lastName.Length > 50) throw new ArgumentException("Last name cannot exceed 50 characters", nameof(lastName));
            if (email.Length > 254) throw new ArgumentException("Email cannot exceed 254 characters", nameof(email));
            if (shippingAddress.Length > 100) throw new ArgumentException("Shipping address cannot exceed 100 characters", nameof(shippingAddress));
            if (invoiceAddress.Length > 100) throw new ArgumentException("Invoice address cannot exceed 100 characters", nameof(invoiceAddress));
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateOrderCommand, Order>();
        }
    }
}