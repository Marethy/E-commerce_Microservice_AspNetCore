using Contracts.Common.Interfaces;
using Product.API.Entities;

namespace Product.API.Repositories.Interfaces
{
    public interface ISellerRepository : IRepositoryBase<Seller, Guid>
    {
        Task<Seller?> GetSellerByNameAsync(string name);
        Task<IEnumerable<Seller>> GetSellersAsync();
        Task<IEnumerable<Seller>> GetOfficialSellersAsync();
        Task<bool> SellerExistsAsync(Guid sellerId);
    }
}
