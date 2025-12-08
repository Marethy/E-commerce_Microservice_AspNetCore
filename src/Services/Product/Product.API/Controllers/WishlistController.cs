using AutoMapper;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using Shared.SeedWork.ApiResult;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WishlistController : ControllerBase
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<WishlistController> logger)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
      ?? User.FindFirstValue("sub") 
    ?? User.Identity?.Name;
     
        if (string.IsNullOrEmpty(userId))
 {
   throw new UnauthorizedAccessException("User ID not found in token");
        }
   
        return userId;
    }

    /// <summary>
    /// Get user's wishlist with pagination
    /// </summary>
    [HttpGet]
  [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<WishlistDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<WishlistDto>>> GetWishlist(
        [FromQuery] int page = 0,
   [FromQuery] int limit = 20)
    {
        try
        {
          var userId = GetCurrentUserId();
  var (items, totalCount) = await _wishlistRepository.GetUserWishlistAsync(userId, page, limit);

            var wishlistItems = items.Select(w =>
            {
                var dto = new WishlistItemDto
                {
             Id = w.Id,
             ProductId = w.ProductId,
         ProductNo = w.Product.No,
          ProductName = w.Product.Name,
 Price = w.Product.Price,
      OriginalPrice = w.OriginalPrice,
        AddedDate = w.AddedDate,
          IsInStock = w.Product.InventoryStatus == "IN_STOCK",
      InventoryStatus = w.Product.InventoryStatus,
     CurrentPrice = w.Product.Price,
  BrandName = w.Product.Brand?.Name,
  RatingAverage = w.Product.RatingAverage,
      ReviewCount = w.Product.ReviewCount
                };

         // Calculate price change
    if (w.OriginalPrice != w.Product.Price)
          {
        dto.PriceChanged = true;
  dto.PriceDropPercentage = Math.Round(
          ((w.OriginalPrice - w.Product.Price) / w.OriginalPrice) * 100, 2);
                }

     // Get primary image
              var primaryImage = w.Product.Images?.FirstOrDefault(i => i.IsPrimary);
  dto.PrimaryImageUrl = primaryImage?.Url;

  return dto;
            }).ToList();

            var result = new WishlistDto
            {
           Items = wishlistItems,
       Total = totalCount,
            Page = page,
       Limit = limit,
       TotalPages = (int)Math.Ceiling(totalCount / (double)limit)
            };

            return Ok(new ApiSuccessResult<WishlistDto>(result));
        }
        catch (UnauthorizedAccessException ex)
    {
          return Unauthorized(new ApiErrorResult<WishlistDto>(ex.Message));
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Error getting wishlist");
    return StatusCode(500, new ApiErrorResult<WishlistDto>("An error occurred while retrieving wishlist"));
  }
  }

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
  public async Task<ActionResult<ApiResult<object>>> AddToWishlist([FromBody] AddToWishlistDto dto)
    {
        try
        {
    var userId = GetCurrentUserId();

         // Check if product exists
       var product = await _productRepository.GetProduct(dto.ProductId);
            if (product == null)
            {
            return NotFound(new ApiErrorResult<object>($"Product with ID {dto.ProductId} not found"));
            }

            // Check if already in wishlist
        var exists = await _wishlistRepository.IsInWishlistAsync(userId, dto.ProductId);
      if (exists)
    {
    return Conflict(new ApiErrorResult<object>("Product already in wishlist"));
            }

 // Add to wishlist
      var wishlistItem = new Wishlist
          {
        UserId = userId,
         ProductId = dto.ProductId,
           OriginalPrice = product.Price,
   AddedDate = DateTimeOffset.UtcNow
          };

    await _wishlistRepository.CreateAsync(wishlistItem);

            return CreatedAtAction(
          nameof(GetWishlist), 
                null, 
    new ApiSuccessResult<object>(new { id = wishlistItem.Id }, "Product added to wishlist"));
      }
  catch (UnauthorizedAccessException ex)
        {
  return Unauthorized(new ApiErrorResult<object>(ex.Message));
        }
        catch (Exception ex)
  {
       _logger.LogError(ex, "Error adding to wishlist");
            return StatusCode(500, new ApiErrorResult<object>("An error occurred while adding to wishlist"));
     }
    }

    /// <summary>
    /// Remove product from wishlist
    /// </summary>
    [HttpDelete("{productId:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> RemoveFromWishlist([Required] Guid productId)
    {
  try
  {
 var userId = GetCurrentUserId();

            var wishlistItem = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (wishlistItem == null)
        {
     return NotFound(new ApiErrorResult<object>("Product not found in wishlist"));
      }

          await _wishlistRepository.DeleteAsync(wishlistItem);

            return NoContent();
        }
     catch (UnauthorizedAccessException ex)
     {
         return Unauthorized(new ApiErrorResult<object>(ex.Message));
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error removing from wishlist");
            return StatusCode(500, new ApiErrorResult<object>("An error occurred while removing from wishlist"));
        }
    }

    /// <summary>
    /// Clear entire wishlist
    /// </summary>
    [HttpDelete]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
  public async Task<ActionResult> ClearWishlist()
    {
        try
        {
 var userId = GetCurrentUserId();
            await _wishlistRepository.ClearWishlistAsync(userId);
            return NoContent();
        }
 catch (UnauthorizedAccessException ex)
        {
   return Unauthorized(new ApiErrorResult<object>(ex.Message));
      }
        catch (Exception ex)
        {
         _logger.LogError(ex, "Error clearing wishlist");
    return StatusCode(500, new ApiErrorResult<object>("An error occurred while clearing wishlist"));
        }
    }

    /// <summary>
    /// Get wishlist count
    /// </summary>
 [HttpGet("count")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<WishlistCountDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<WishlistCountDto>>> GetWishlistCount()
    {
    try
 {
            var userId = GetCurrentUserId();
   var count = await _wishlistRepository.GetWishlistCountAsync(userId);

   return Ok(new ApiSuccessResult<WishlistCountDto>(new WishlistCountDto { Count = count }));
        }
      catch (UnauthorizedAccessException ex)
        {
         return Unauthorized(new ApiErrorResult<WishlistCountDto>(ex.Message));
        }
   catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist count");
    return StatusCode(500, new ApiErrorResult<WishlistCountDto>("An error occurred while getting wishlist count"));
  }
    }

    /// <summary>
  /// Check if product is in wishlist
    /// </summary>
    [HttpGet("check/{productId:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<WishlistStatusDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<WishlistStatusDto>>> CheckWishlistStatus([Required] Guid productId)
    {
        try
        {
    var userId = GetCurrentUserId();
            var isInWishlist = await _wishlistRepository.IsInWishlistAsync(userId, productId);

            return Ok(new ApiSuccessResult<WishlistStatusDto>(
    new WishlistStatusDto { IsInWishlist = isInWishlist }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiErrorResult<WishlistStatusDto>(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking wishlist status");
 return StatusCode(500, new ApiErrorResult<WishlistStatusDto>("An error occurred while checking wishlist status"));
   }
    }

    /// <summary>
    /// Get wishlist with current prices (price tracking)
    /// </summary>
    [HttpGet("with-prices")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
  [ProducesResponseType(typeof(ApiResult<List<WishlistItemDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<WishlistItemDto>>>> GetWishlistWithPrices()
    {
        try
        {
       var userId = GetCurrentUserId();
var (items, _) = await _wishlistRepository.GetUserWishlistAsync(userId, 0, 1000);

      var wishlistItems = items.Select(w =>
     {
         var dto = new WishlistItemDto
         {
       Id = w.Id,
   ProductId = w.ProductId,
      ProductName = w.Product.Name,
  OriginalPrice = w.OriginalPrice,
     CurrentPrice = w.Product.Price,
     IsInStock = w.Product.InventoryStatus == "IN_STOCK",
  AddedDate = w.AddedDate
         };

       // Calculate price change
     if (w.OriginalPrice != w.Product.Price)
         {
                dto.PriceChanged = true;
          var priceDrop = w.OriginalPrice - w.Product.Price;
           dto.PriceDropPercentage = Math.Round((priceDrop / w.OriginalPrice) * 100, 2);
   }

            return dto;
            }).ToList();

          return Ok(new ApiSuccessResult<List<WishlistItemDto>>(wishlistItems));
      }
   catch (UnauthorizedAccessException ex)
      {
         return Unauthorized(new ApiErrorResult<List<WishlistItemDto>>(ex.Message));
      }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Error getting wishlist with prices");
    return StatusCode(500, new ApiErrorResult<List<WishlistItemDto>>("An error occurred"));
        }
    }

    /// <summary>
    /// Get wishlist analytics
    /// </summary>
    [HttpGet("analytics/stats")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<WishlistAnalyticsDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<WishlistAnalyticsDto>>> GetWishlistAnalytics()
    {
  try
        {
          var userId = GetCurrentUserId();
            var total = await _wishlistRepository.GetWishlistCountAsync(userId);
          var recent = await _wishlistRepository.GetRecentWishlistCountAsync(userId, 7);

    var analytics = new WishlistAnalyticsDto
 {
     Total = total,
   Recent = recent
     };

            return Ok(new ApiSuccessResult<WishlistAnalyticsDto>(analytics));
        }
 catch (UnauthorizedAccessException ex)
        {
     return Unauthorized(new ApiErrorResult<WishlistAnalyticsDto>(ex.Message));
        }
        catch (Exception ex)
 {
     _logger.LogError(ex, "Error getting wishlist analytics");
     return StatusCode(500, new ApiErrorResult<WishlistAnalyticsDto>("An error occurred"));
        }
    }

    /// <summary>
    /// Get most wishlisted products (for trending/popular sections)
    /// </summary>
    [HttpGet("analytics/most-wishlisted")]
 [AllowAnonymous] // Public endpoint for trending products
    [ProducesResponseType(typeof(ApiResult<List<MostWishlistedProductDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<MostWishlistedProductDto>>>> GetMostWishlisted(
        [FromQuery] int limit = 10)
    {
        try
        {
            var mostWishlisted = await _wishlistRepository.GetMostWishlistedProductsAsync(limit);

      var results = new List<MostWishlistedProductDto>();

            foreach (var (productId, count) in mostWishlisted)
 {
         var product = await _productRepository.GetProduct(productId);
         if (product != null)
    {
   var primaryImage = product.Images?.FirstOrDefault(i => i.IsPrimary);
     results.Add(new MostWishlistedProductDto
      {
             ProductId = productId,
            ProductNo = product.No,
        ProductName = product.Name,
         WishlistCount = count,
       Price = product.Price,
         PrimaryImageUrl = primaryImage?.Url
  });
           }
    }

         return Ok(new ApiSuccessResult<List<MostWishlistedProductDto>>(results));
        }
   catch (Exception ex)
        {
_logger.LogError(ex, "Error getting most wishlisted products");
 return StatusCode(500, new ApiErrorResult<List<MostWishlistedProductDto>>("An error occurred"));
 }
    }
}
