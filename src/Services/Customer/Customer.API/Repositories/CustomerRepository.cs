using Customer.API.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Customer.API.Repositories.Interfaces
{
    public class CustomerRepository : RepositoryQueryBase<Entities.Customer, int, CustomerContext>, ICustomerRepository
    {
        public CustomerRepository(CustomerContext dbContext) : base(dbContext)
        {
        }

        public async Task<Entities.Customer> GetCustomerByUserNameAsync(string username)
        {
            return await FindByCondition(x => x.UserName == username).FirstOrDefaultAsync();
        }

        public async Task<Entities.Customer> GetCustomerByEmailAsync(string email)
        {
            return await FindByCondition(x => x.Email == email).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Entities.Customer>> GetCustomersAsync()
        {
            return await FindAll().ToListAsync();
        }
    }
}