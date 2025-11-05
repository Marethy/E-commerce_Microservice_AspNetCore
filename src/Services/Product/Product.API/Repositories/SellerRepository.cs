using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;

namespace Product.API.Repositories
{
    public class SellerRepository : RepositoryBase<Seller, Guid, ProductContext>, ISellerRepository
    {
        public SellerRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) 
            : base(dbContext, unitOfWork)
        {
        }

        public async Task<Seller?> GetSellerByNameAsync(string name)
        {
            return await FindByCondition(x => x.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Seller>> GetSellersAsync()
        {
            return await FindAll()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seller>> GetOfficialSellersAsync()
        {
            return await FindByCondition(x => x.IsOfficial)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<bool> SellerExistsAsync(Guid sellerId)
        {
            return await FindByCondition(x => x.Id == sellerId)
                .AnyAsync();
        }
    }
}
