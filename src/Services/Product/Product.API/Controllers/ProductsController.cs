using AutoMapper;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Product.API.Services.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using Shared.SeedWork.ApiResult;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IBrandRepository _brandRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly IClipSearchService _clipSearchService;
    private readonly IMapper _mapper;
    private readonly Persistence.ProductContext _context;

    public ProductsController(
        IProductRepository repository,
        IBrandRepository brandRepository,
        ISellerRepository sellerRepository,
        IClipSearchService clipSearchService,
        IMapper mapper,
        Persistence.ProductContext context)
    {
        _repository = repository;
        _brandRepository = brandRepository;
        _sellerRepository = sellerRepository;
        _clipSearchService = clipSearchService;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductDto>>>> GetProducts([FromQuery] Guid? categoryId = null)
    {
        var products = categoryId.HasValue
            ? await _repository.GetProductsByCategory(categoryId.Value)
            : await _repository.GetProducts();

 var result = _mapper.Map<List<ProductDto>>(products);
        return Ok(new ApiSuccessResult<List<ProductDto>>(result));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PagedProductResponse>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<PagedProductResponse>>> SearchProducts(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        [FromQuery] bool? discount = null,
        [FromQuery] int? minDiscount = null)
    {
        // Apply discount filter if provided
        if (discount.HasValue && discount.Value)
        {
            filter.HasDiscount = true;
        }

        // Apply minimum discount percentage filter if provided
        if (minDiscount.HasValue && minDiscount.Value > 0)
        {
            filter.MinDiscountPercentage = minDiscount.Value;
        }

        if (!string.IsNullOrEmpty(filter.Q))
        {
            var (productIds, totalFromElastic) = await _clipSearchService.SearchProductIdsAsync(filter.Q, page, size);
            
            if (productIds.Count == 0)
            {
                return Ok(new ApiSuccessResult<PagedProductResponse>(new PagedProductResponse
                {
                    Content = new List<ProductDto>(),
                    Meta = new PageMetadata { Page = page, Size = size, TotalElements = 0, TotalPages = 0, Last = true }
                }));
            }
            
            filter.ProductIds = productIds;
            var (products, _) = await _repository.SearchProducts(filter, 0, productIds.Count);
            
            var productList = products.ToList();
            var orderedProducts = productIds
                .Select(id => productList.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .ToList();
            
            var productDtos = _mapper.Map<List<ProductDto>>(orderedProducts);
            
            var response = new PagedProductResponse
            {
                Content = productDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Size = size,
                    TotalElements = totalFromElastic,
                    TotalPages = (int)Math.Ceiling(totalFromElastic / (double)size),
                    Last = (page + 1) * size >= totalFromElastic
                }
            };

            return Ok(new ApiSuccessResult<PagedProductResponse>(response));
        }
        
        var (allProducts, total) = await _repository.SearchProducts(filter, page, size);
        var allProductDtos = _mapper.Map<List<ProductDto>>(allProducts);
        
        var responseNoQuery = new PagedProductResponse
        {
            Content = allProductDtos,
            Meta = new PageMetadata
            {
                Page = page,
                Size = size,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Last = (page + 1) * size >= total
            }
        };

        return Ok(new ApiSuccessResult<PagedProductResponse>(responseNoQuery));
    }

    [HttpPost("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PagedProductResponse>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<PagedProductResponse>>> SearchProductsAdvanced([FromBody] ProductSearchRequestDto request)
    {
        var filter = request.Filter ?? new ProductFilterDto();
        var page = request.Page;
        var size = request.Size;

        // Check if we have query or image for CLIP search
        if (!string.IsNullOrEmpty(request.Query) || !string.IsNullOrEmpty(request.ImageBase64))
        {
            byte[]? imageBytes = null;
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    imageBytes = Convert.FromBase64String(request.ImageBase64);
                }
                catch (FormatException)
                {
                    return BadRequest(new ApiErrorResult<PagedProductResponse>("Invalid base64 image format"));
                }
            }

            var (productIds, totalFromElastic) = await _clipSearchService.SearchProductIdsAsync(request.Query, page, size, imageBytes);
            
            if (productIds.Count == 0)
            {
                return Ok(new ApiSuccessResult<PagedProductResponse>(new PagedProductResponse
                {
                    Content = new List<ProductDto>(),
                    Meta = new PageMetadata { Page = page, Size = size, TotalElements = 0, TotalPages = 0, Last = true }
                }));
            }
            
            filter.ProductIds = productIds;
            var (products, _) = await _repository.SearchProducts(filter, 0, productIds.Count);
            
            var productList = products.ToList();
            var orderedProducts = productIds
                .Select(id => productList.FirstOrDefault(p => p.Id == id))
                .Where(p => p != null)
                .ToList();
            
            var productDtos = _mapper.Map<List<ProductDto>>(orderedProducts);
            
            var response = new PagedProductResponse
            {
                Content = productDtos,
                Meta = new PageMetadata
                {
                    Page = page,
                    Size = size,
                    TotalElements = totalFromElastic,
                    TotalPages = (int)Math.Ceiling(totalFromElastic / (double)size),
                    Last = (page + 1) * size >= totalFromElastic
                }
            };

            return Ok(new ApiSuccessResult<PagedProductResponse>(response));
        }
        
        // No query or image, use regular filter search
        var (allProducts, total) = await _repository.SearchProducts(filter, page, size);
        var allProductDtos = _mapper.Map<List<ProductDto>>(allProducts);
        
        var responseNoQuery = new PagedProductResponse
        {
            Content = allProductDtos,
            Meta = new PageMetadata
            {
                Page = page,
                Size = size,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Last = (page + 1) * size >= total
            }
        };

        return Ok(new ApiSuccessResult<PagedProductResponse>(responseNoQuery));
    }

    [HttpGet("top-rated")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PagedProductResponse>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<PagedProductResponse>>> GetTopRatedProducts(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        filter.SortBy = "rating";
        filter.SortDirection = "desc";
        
        var (products, total) = await _repository.SearchProducts(filter, page, size);
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        
        var response = new PagedProductResponse
        {
            Content = productDtos,
            Meta = new PageMetadata
            {
                Page = page,
                Size = size,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Last = (page + 1) * size >= total
            }
        };

        return Ok(new ApiSuccessResult<PagedProductResponse>(response));
    }

    [HttpGet("new-arrivals")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PagedProductResponse>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<PagedProductResponse>>> GetNewArrivals(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        filter.SortBy = "created";
        filter.SortDirection = "desc";
        
        var (products, total) = await _repository.SearchProducts(filter, page, size);
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        
        var response = new PagedProductResponse
        {
            Content = productDtos,
            Meta = new PageMetadata
            {
                Page = page,
                Size = size,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Last = (page + 1) * size >= total
            }
        };

        return Ok(new ApiSuccessResult<PagedProductResponse>(response));
    }

    [HttpGet("top-selling")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<PagedProductResponse>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<PagedProductResponse>>> GetTopSellingProducts(
        [FromQuery] ProductFilterDto filter,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20)
    {
        filter.SortBy = "sales";
        filter.SortDirection = "desc";
        
        var (products, total) = await _repository.SearchProducts(filter, page, size);
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        
        var response = new PagedProductResponse
        {
            Content = productDtos,
            Meta = new PageMetadata
            {
                Page = page,
                Size = size,
                TotalElements = total,
                TotalPages = (int)Math.Ceiling(total / (double)size),
                Last = (page + 1) * size >= total
            }
        };

        return Ok(new ApiSuccessResult<PagedProductResponse>(response));
    }

    [HttpGet("summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductSummaryDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductSummaryDto>>>> GetProductsSummary([FromQuery] Guid? categoryId = null)
  {
        var products = categoryId.HasValue
 ? await _repository.GetProductsByCategory(categoryId.Value)
    : await _repository.GetProducts();

        var result = _mapper.Map<List<ProductSummaryDto>>(products);
        return Ok(new ApiSuccessResult<List<ProductSummaryDto>>(result));
    }

    [HttpGet("statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<ProductStatisticsDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<ProductStatisticsDto>>> GetProductStatistics()
    {
        var totalProducts = await _context.Products.CountAsync();
        var inStockProducts = await _context.Products.CountAsync(p => p.InventoryStatus != "OUT_OF_STOCK");
        var outOfStockProducts = await _context.Products.CountAsync(p => p.InventoryStatus == "OUT_OF_STOCK");
        var totalRevenue = await _context.Products.SumAsync(p => (decimal)p.Price * p.AllTimeQuantitySold);

        var result = new ProductStatisticsDto
        {
            TotalProducts = totalProducts,
            InStockProducts = inStockProducts,
            OutOfStockProducts = outOfStockProducts,
            TotalRevenue = totalRevenue
        };

        return Ok(new ApiSuccessResult<ProductStatisticsDto>(result));
    }

 [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductDto>>> GetProductById([Required] Guid id)
    {
        var product = await _repository.GetProduct(id);
if (product == null)
     return NotFound(new ApiErrorResult<ProductDto>($"Product with ID {id} not found"));

   var result = _mapper.Map<ProductDto>(product);
        return Ok(new ApiSuccessResult<ProductDto>(result));
  }

    [HttpGet("by-no/{productNo}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductDto>>> GetProductByNo([Required] string productNo)
    {
     var product = await _repository.GetProductByNo(productNo);
if (product == null)
      return NotFound(new ApiErrorResult<ProductDto>($"Product No '{productNo}' not found"));

        var result = _mapper.Map<ProductDto>(product);
        return Ok(new ApiSuccessResult<ProductDto>(result));
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductDto>>> GetProductBySlug([Required] string slug)
    {
        var product = await _repository.GetProductBySlug(slug);
        if (product == null)
            return NotFound(new ApiErrorResult<ProductDto>($"Product with slug '{slug}' not found"));

        var result = _mapper.Map<ProductDto>(product);
        return Ok(new ApiSuccessResult<ProductDto>(result));
    }

    [HttpGet("{id:guid}/images")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductImageDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<List<ProductImageDto>>>> GetProductImages([Required] Guid id)
    {
        var product = await _repository.GetProduct(id);
        if (product == null)
            return NotFound(new ApiErrorResult<List<ProductImageDto>>($"Product with ID {id} not found"));

        var images = await _repository.GetProductImages(id);
        var result = _mapper.Map<List<ProductImageDto>>(images);
        return Ok(new ApiSuccessResult<List<ProductImageDto>>(result));
    }

    [HttpGet("category/{categoryId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductDto>>>> GetProductsByCategory([Required] Guid categoryId)
    {
        var products = await _repository.GetProductsByCategory(categoryId);
        var result = _mapper.Map<List<ProductDto>>(products);
        return Ok(new ApiSuccessResult<List<ProductDto>>(result));
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<ActionResult<ApiResult<ProductDto>>> CreateProduct([FromBody] CreateProductDto productDto)
    {
        var existingProduct = await _repository.GetProductByNo(productDto.No);
   if (existingProduct != null)
     return Conflict(new ApiErrorResult<ProductDto>($"Product No '{productDto.No}' already exists"));

      var categoryExists = await _repository.CategoryExists(productDto.CategoryId);
        if (!categoryExists)
      return BadRequest(new ApiErrorResult<ProductDto>($"Category ID {productDto.CategoryId} not found"));

        if (productDto.BrandId.HasValue)
   {
        var brandExists = await _brandRepository.BrandExistsAsync(productDto.BrandId.Value);
     if (!brandExists)
           return BadRequest(new ApiErrorResult<ProductDto>($"Brand ID {productDto.BrandId} not found"));
        }

     if (productDto.SellerId.HasValue)
  {
   var sellerExists = await _sellerRepository.SellerExistsAsync(productDto.SellerId.Value);
if (!sellerExists)
              return BadRequest(new ApiErrorResult<ProductDto>($"Seller ID {productDto.SellerId} not found"));
   }

        var product = _mapper.Map<CatalogProduct>(productDto);
        var productId = await _repository.CreateAsync(product);

        var result = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetProductById), new { id = productId }, new ApiSuccessResult<ProductDto>(result));
  }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    [ProducesResponseType(typeof(ApiResult<ProductDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductDto>>> UpdateProduct([Required] Guid id, [FromBody] UpdateProductDto productDto)
    {
        var product = await _repository.GetProductForUpdate(id);
        if (product == null)
            return NotFound(new ApiErrorResult<ProductDto>($"Product with ID {id} not found"));

        if (productDto.CategoryId.HasValue)
        {
     var categoryExists = await _repository.CategoryExists(productDto.CategoryId.Value);
            if (!categoryExists)
         return BadRequest(new ApiErrorResult<ProductDto>($"Category ID {productDto.CategoryId} not found"));
        }

        if (productDto.BrandId.HasValue)
        {
 var brandExists = await _brandRepository.BrandExistsAsync(productDto.BrandId.Value);
  if (!brandExists)
           return BadRequest(new ApiErrorResult<ProductDto>($"Brand ID {productDto.BrandId} not found"));
        }

    if (productDto.SellerId.HasValue)
        {
         var sellerExists = await _sellerRepository.SellerExistsAsync(productDto.SellerId.Value);
            if (!sellerExists)
   return BadRequest(new ApiErrorResult<ProductDto>($"Seller ID {productDto.SellerId} not found"));
     }

        _mapper.Map(productDto, product);
        await _repository.UpdateAsync(product);

        var result = _mapper.Map<ProductDto>(product);
        return Ok(new ApiSuccessResult<ProductDto>(result));
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteProduct([Required] Guid id)
    {
    var product = await _repository.GetProduct(id);
        if (product == null)
       return NotFound(new ApiErrorResult<object>($"Product with ID {id} not found"));

   await _repository.DeleteAsync(product);
        return NoContent();
    }
}