using Customer.API.Services.Interfaces;

namespace Customer.API.Controllers
{
    public static class CustomerController
    {
        public static void MapCustomerController(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/", () => "Welcome to Customer.API");
            endpoints.MapGet("/api/customers", async (ICustomerService customerService)
                => await customerService.GetCustomersAsync());
            endpoints.MapGet("/api/customers/{username}", async (ICustomerService customerService, string username)
                => await customerService.GetCustomerByUserNameAsync(username));
        }
    }
}