using AutoMapper;
using Ordering.Application.Common.Mappings;
using Ordering.Domain.Entities;
using Ordering.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Application.Common.Models
{
    public class OrderDto : IMapFrom<Order>
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        //Adress
        public string ShippingAddress { get; set; }
        public string InvoiceAddress { get; set; }

        public EOrderStatus Status { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Order, OrderDto>().ReverseMap();

        }

    }
   

}