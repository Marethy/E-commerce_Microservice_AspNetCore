using AutoMapper;
using Infrastructure.Mappings;
using Shared.DTOs.Customer;

namespace Customer.API
{
    public class CustomerMappingProfile : Profile
    {
        public CustomerMappingProfile()
        {
            // Customer mappings
            CreateMap<Customer.API.Entities.Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer.API.Entities.Customer>();
            CreateMap<UpdateCustomerDto, Customer.API.Entities.Customer>().IgnoreAllNonExisting();
            CreateMap<Customer.API.Entities.Customer, CustomerProfileDto>();
        }
    }
}