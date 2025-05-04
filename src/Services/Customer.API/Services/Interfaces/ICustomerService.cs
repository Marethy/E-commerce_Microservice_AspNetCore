namespace Customer.API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IResult> GetCustomerByUserNameAsync(string username);

        Task<IResult> GetCustomersAsync();
    }
}