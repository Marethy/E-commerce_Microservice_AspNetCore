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
            // Product mappings
            CreateMap<CatalogProduct, ProductDto>();
            CreateMap<CreateProductDto, CatalogProduct>();
            CreateMap<UpdateProductDto, CatalogProduct>().IgnoreAllNonExisting();

            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>().IgnoreAllNonExisting();

            // ProductReview mappings
            CreateMap<ProductReview, ProductReviewDto>();
            CreateMap<CreateProductReviewDto, ProductReview>();
            CreateMap<UpdateProductReviewDto, ProductReview>().IgnoreAllNonExisting();
        }
    }
}