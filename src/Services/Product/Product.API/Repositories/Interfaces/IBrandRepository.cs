using Contracts.Common.Interfaces;
using Product.API.Entities;

namespace Product.API.Repositories.Interfaces
{
    public interface IBrandRepository : IRepositoryBase<Brand, Guid>
    {
        Task<Brand?> GetBrandByNameAsync(string name);
        Task<Brand?> GetBrandBySlugAsync(string slug);
        Task<IEnumerable<Brand>> GetBrandsAsync();
        Task<bool> BrandExistsAsync(Guid brandId);
    }
}
