using Microsoft.EntityFrameworkCore;

namespace Customer.API.Persistence
{
    public static class CustomerContextSeed
    {
        public static async Task<IHost> SeedCustomerDataAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var customerContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();

            await customerContext.Database.MigrateAsync().ConfigureAwait(false);

            await TryAddCustomerAsync(customerContext, "customer1", "customer1", "customer", "customer1@local.com");
            await TryAddCustomerAsync(customerContext, "customer2", "customer2", "customer", "customer2@local.com");

            return host;
        }

        private static async Task TryAddCustomerAsync(CustomerContext customerContext, string username, string firstName, string lastName, string email)
        {
            bool exists = await customerContext.Customers
                .AnyAsync(x => x.UserName == username || x.Email == email)
                .ConfigureAwait(false);

            if (!exists)
            {
                var newCustomer = new Entities.Customer
                {
                    UserName = username,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email
                };

                await customerContext.Customers.AddAsync(newCustomer).ConfigureAwait(false);
                await customerContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
