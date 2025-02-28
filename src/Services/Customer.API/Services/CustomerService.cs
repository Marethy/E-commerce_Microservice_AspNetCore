using Customer.API.Repositories.Interfaces;
using Customer.API.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Customer.API.Services
{
    public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository = customerRepository;

        public async Task<IResult> GetCustomerByUserNameAsync(string username)
        {
            var customer = await _customerRepository.GetCustomerByUserNameAsync(username);
            if (customer == null)
            {
                return Results.NotFound($"Customer with username {username} not found.");
            }
            return Results.Ok(customer);
        }

        public async Task<IResult> GetCustomersAsync()
        {
            return Results.Ok(await _customerRepository.GetCustomersAsync());
        }
    }
}
