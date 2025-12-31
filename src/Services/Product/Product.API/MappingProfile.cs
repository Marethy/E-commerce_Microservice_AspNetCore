using AutoMapper;
using Infrastructure.Mappings;
using Product.API.Entities;
using Shared.DTOs.Product;

namespace Product.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Global type converter for DateTimeOffset -> DateTime
            CreateMap<DateTimeOffset, DateTime>().ConvertUsing(src => src.DateTime);
            CreateMap<DateTimeOffset?, DateTime?>().ConvertUsing(src => src.HasValue ? src.Value.DateTime : null);
            
            // Product mappings
            CreateMap<CatalogProduct, ProductDto>()
                .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.Summary))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Name : null))
                .ForMember(dest => dest.IsSellerOfficial, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.IsOfficial : (bool?)null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Specifications));

            CreateMap<CatalogProduct, ProductSummaryDto>()
                .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.Summary))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
                .ForMember(dest => dest.IsSellerOfficial, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.IsOfficial : (bool?)null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src => 
                    src.Images.OrderBy(i => i.Position).FirstOrDefault(i => i.IsPrimary) != null 
                        ? src.Images.OrderBy(i => i.Position).FirstOrDefault(i => i.IsPrimary)!.Url 
                        : src.Images.OrderBy(i => i.Position).FirstOrDefault() != null 
                            ? src.Images.OrderBy(i => i.Position).FirstOrDefault()!.Url 
                            : null));

            CreateMap<CreateProductDto, CatalogProduct>()
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => src.Specifications));

            CreateMap<UpdateProductDto, CatalogProduct>().IgnoreAllNonExisting();

            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>().IgnoreAllNonExisting();

            // ProductReview mappings
            CreateMap<ProductReview, ProductReviewDto>();
            CreateMap<CreateProductReviewDto, ProductReview>()
                .ForMember(dest => dest.ReviewDate, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
                .ForMember(dest => dest.HelpfulVotes, opt => opt.MapFrom(src => 0));
            CreateMap<UpdateProductReviewDto, ProductReview>().IgnoreAllNonExisting();

            // Brand mappings
            CreateMap<Brand, BrandDto>();
            CreateMap<CreateBrandDto, Brand>();

            // Seller mappings
            CreateMap<Seller, SellerDto>();
            CreateMap<CreateSellerDto, Seller>();

            // ProductImage mappings
            CreateMap<ProductImage, ProductImageDto>();
            CreateMap<CreateProductImageDto, ProductImage>();

            // ProductSpecification mappings
            CreateMap<ProductSpecification, ProductSpecificationDto>();
            CreateMap<CreateProductSpecificationDto, ProductSpecification>();
        }
    }
}