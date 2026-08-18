using Mapster;
using Product.API.Entities;
using Shared.DTOs.Product;

namespace Product.API
{
    public class MappingRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Global type converters for DateTimeOffset -> DateTime
            config.NewConfig<DateTimeOffset, DateTime>()
                .MapWith(src => src.DateTime);
            config.NewConfig<DateTimeOffset?, DateTime?>()
                .MapWith(src => src.HasValue ? src.Value.DateTime : (DateTime?)null);

            // CatalogProduct -> ProductDto
            config.NewConfig<CatalogProduct, ProductDto>()
                .Map(dest => dest.ShortDescription, src => src.Summary)
                .Map(dest => dest.CategoryName, src => src.Category.Name)
                .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
                .Map(dest => dest.SellerName, src => src.Seller != null ? src.Seller.Name : null)
                .Map(dest => dest.IsSellerOfficial, src => src.Seller != null ? src.Seller.IsOfficial : (bool?)null);

            // CatalogProduct -> ProductSummaryDto
            config.NewConfig<CatalogProduct, ProductSummaryDto>()
                .Map(dest => dest.ShortDescription, src => src.Summary)
                .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : null)
                .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : null)
                .Map(dest => dest.IsSellerOfficial, src => src.Seller != null ? src.Seller.IsOfficial : (bool?)null)
                .Map(dest => dest.PrimaryImageUrl, src => GetPrimaryImageUrl(src.Images));

            // CreateProductReviewDto -> ProductReview (defaults for computed fields)
            config.NewConfig<CreateProductReviewDto, ProductReview>()
                .Map(dest => dest.ReviewDate, src => DateTimeOffset.UtcNow)
                .Map(dest => dest.HelpfulVotes, src => 0);
        }

        private static string? GetPrimaryImageUrl(ICollection<ProductImage>? images)
        {
            if (images == null || images.Count == 0) return null;
            var ordered = images.OrderBy(i => i.Position).ToList();
            var primary = ordered.FirstOrDefault(i => i.IsPrimary) ?? ordered.FirstOrDefault();
            return primary?.Url;
        }
    }
}
