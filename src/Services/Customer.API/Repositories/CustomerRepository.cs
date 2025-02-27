using Contracts.Common.Interfaces;
using Customer.API.Persistence;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Customer.API.Repositories.Interfaces
{
    public class CustomerRepository : RepositoryBaseAsync<Entities.Customer, int, CustomerContext>, ICustomerRepository
    {
        public CustomerRepository(CustomerContext dbContext, IUnitOfWork<CustomerContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public async Task<Entities.Customer> GetCustomerByUserNameAsync(string username)
        {
            return await FindByCondition(x => x.UserName == username).FirstOrDefaultAsync();
        }
    }
}
