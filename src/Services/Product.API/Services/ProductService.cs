using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Services
{
    public class ProductService
    {
        private readonly IUnitOfWork<ProductContext> _unitOfWork;

        public ProductService(IUnitOfWork<ProductContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Example: Create product with category validation using UnitOfWork pattern
        public async Task<Guid> CreateProductWithCategoryAsync(CatalogProduct product)
        {
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Get repositories from UnitOfWork factory
                var productRepo = _unitOfWork.GetRepository<CatalogProduct, Guid>();
                var categoryRepo = _unitOfWork.GetRepository<Category, Guid>();

                // Validate category exists
                var category = await categoryRepo.GetByIdAsync(product.CategoryId);
                if (category == null)
                    throw new ArgumentException("Category not found");

                // Create product
                var productId = await productRepo.CreateAsync(product);
                
                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();
                
                return productId;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // Example: Batch operations with transaction
        public async Task<bool> CreateProductsWithReviewsAsync(
            IEnumerable<CatalogProduct> products, 
            IEnumerable<ProductReview> reviews)
        {
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                var productRepo = _unitOfWork.GetRepository<CatalogProduct, Guid>();
                var reviewRepo = _unitOfWork.GetRepository<ProductReview, Guid>();

                // Create products
                await productRepo.CreateListAsync(products);
                
                // Create reviews
                await reviewRepo.CreateListAsync(reviews);
                
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
        }
    }
}