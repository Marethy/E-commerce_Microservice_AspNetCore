using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;

namespace Product.API.Repositories
{
    public class BrandRepository : RepositoryBase<Brand, Guid, ProductContext>, IBrandRepository
    {
        public BrandRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) 
            : base(dbContext, unitOfWork)
        {
        }

        public async Task<Brand?> GetBrandByNameAsync(string name)
        {
            return await FindByCondition(x => x.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<Brand?> GetBrandBySlugAsync(string slug)
        {
            return await FindByCondition(x => x.Slug.ToLower() == slug.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync()
        {
            return await FindAll()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> BrandExistsAsync(Guid brandId)
        {
            return await FindByCondition(x => x.Id == brandId)
                .AnyAsync();
        }
    }
}
