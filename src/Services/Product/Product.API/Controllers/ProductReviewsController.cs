using Infrastructure.Identity.Authorization;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Product.API.Services.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using Shared.SeedWork.ApiResult;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IProductStatsService _statsService;

    public ProductReviewsController(
        IProductReviewRepository repository,
  IProductRepository productRepository,
    IProductStatsService statsService)
    {
    _repository = repository;
        _productRepository = productRepository;
        _statsService = statsService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductReviewDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductReviewDto>>>> GetAllReviews()
    {
        var reviews = await _repository.GetAllReviewsAsync();
        var result = reviews.Adapt<List<ProductReviewDto>>();
        return Ok(new ApiSuccessResult<List<ProductReviewDto>>(result));
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<object>>> GetReviewsByProduct(
        [Required] Guid productId,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? order = null)
    {
        var allReviews = await _repository.GetReviewsByProduct(productId);
        
        // Convert to List for manipulation
        List<ProductReview> filteredReviews = allReviews.ToList();

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            var sortByLower = sortBy.ToLower();
            var isAscending = order?.ToLower() == "asc";

            if (sortByLower == "rating")
            {
                filteredReviews = isAscending
                    ? filteredReviews.OrderBy(r => r.Rating).ToList()
                    : filteredReviews.OrderByDescending(r => r.Rating).ToList();
            }
            else if (sortByLower == "date" || sortByLower == "reviewdate")
            {
                filteredReviews = isAscending
                    ? filteredReviews.OrderBy(r => r.ReviewDate).ToList()
                    : filteredReviews.OrderByDescending(r => r.ReviewDate).ToList();
            }
            else if (sortByLower == "helpful" || sortByLower == "helpfulvotes")
            {
                filteredReviews = isAscending
                    ? filteredReviews.OrderBy(r => r.HelpfulVotes).ToList()
                    : filteredReviews.OrderByDescending(r => r.HelpfulVotes).ToList();
            }
            else
            {
                filteredReviews = filteredReviews.OrderByDescending(r => r.ReviewDate).ToList();
            }
        }
        else
        {
            // Default: sort by date descending
            filteredReviews = filteredReviews.OrderByDescending(r => r.ReviewDate).ToList();
        }

        // Apply pagination
        var totalElements = filteredReviews.Count;
        var totalPages = (int)Math.Ceiling(totalElements / (double)size);
        var paginatedReviews = filteredReviews
            .Skip(page * size)
            .Take(size)
            .ToList();

        var reviewDtos = paginatedReviews.Adapt<List<ProductReviewDto>>();

        var result = new
        {
            content = reviewDtos,
            page,
            size,
            totalElements,
            totalPages,
            last = page >= totalPages - 1
        };

        return Ok(new ApiSuccessResult<object>(result));
 }

    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductReviewDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductReviewDto>>>> GetReviewsByUser([Required] string userId)
    {
        var reviews = await _repository.GetReviewsByUser(userId);
   var result = reviews.Adapt<List<ProductReviewDto>>();
        return Ok(new ApiSuccessResult<List<ProductReviewDto>>(result));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<ProductReviewDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductReviewDto>>> GetReviewById([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
         return NotFound(new ApiErrorResult<ProductReviewDto>($"Review with ID {id} not found"));

        var result = review.Adapt<ProductReviewDto>();
        return Ok(new ApiSuccessResult<ProductReviewDto>(result));
    }

    [HttpGet("product/{productId:guid}/statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
  public async Task<ActionResult<ApiResult<object>>> GetProductReviewStatistics([Required] Guid productId)
    {
        var averageRating = await _repository.GetAverageRatingByProduct(productId);
 var reviewCount = await _repository.GetReviewCountByProduct(productId);

        // ✅ Get rating breakdown
        var reviews = await _repository.GetReviewsByProduct(productId);
        var ratingBreakdown = new
        {
            oneStar = reviews.Count(r => r.Rating == 1),
            twoStar = reviews.Count(r => r.Rating == 2),
            threeStar = reviews.Count(r => r.Rating == 3),
            fourStar = reviews.Count(r => r.Rating == 4),
            fiveStar = reviews.Count(r => r.Rating == 5)
        };

        var result = new
        {
          averageRating = Math.Round(averageRating, 2),
        totalReviews = reviewCount,
            ratingBreakdown
        };

        return Ok(new ApiSuccessResult<object>(result));
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    [ProducesResponseType(typeof(ApiResult<ProductReviewDto>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<ActionResult<ApiResult<ProductReviewDto>>> CreateReview([FromBody] CreateProductReviewDto reviewDto)
    {
        var product = await _productRepository.GetProduct(reviewDto.ProductId);
      if (product == null)
         return BadRequest(new ApiErrorResult<ProductReviewDto>($"Product with ID {reviewDto.ProductId} not found"));

     var hasReviewed = await _repository.HasUserReviewedProduct(reviewDto.UserId, reviewDto.ProductId);
        if (hasReviewed)
 return Conflict(new ApiErrorResult<ProductReviewDto>("User has already reviewed this product"));

        var review = reviewDto.Adapt<ProductReview>();
        var reviewId = await _repository.CreateAsync(review);

        await _statsService.UpdateProductRatingAsync(reviewDto.ProductId);

        var result = review.Adapt<ProductReviewDto>();
        return CreatedAtAction(nameof(GetReviewById), new { id = reviewId }, new ApiSuccessResult<ProductReviewDto>(result));
 }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    [ProducesResponseType(typeof(ApiResult<ProductReviewDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductReviewDto>>> UpdateReview([Required] Guid id, [FromBody] UpdateProductReviewDto reviewDto)
 {
        var review = await _repository.GetReview(id);
 if (review == null)
            return NotFound(new ApiErrorResult<ProductReviewDto>($"Review with ID {id} not found"));

        var productId = review.ProductId;

     reviewDto.Adapt(review);
        await _repository.UpdateAsync(review);

        await _statsService.UpdateProductRatingAsync(productId);

        var result = review.Adapt<ProductReviewDto>();
   return Ok(new ApiSuccessResult<ProductReviewDto>(result));
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteReview([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
   return NotFound(new ApiErrorResult<object>($"Review with ID {id} not found"));

        var productId = review.ProductId;

        await _repository.DeleteAsync(review);

     await _statsService.UpdateProductRatingAsync(productId);

        return NoContent();
    }

    [HttpPost("{id:guid}/helpful")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<object>>> MarkReviewAsHelpful([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
            return NotFound(new ApiErrorResult<object>($"Review with ID {id} not found"));

        review.HelpfulVotes++;
        await _repository.UpdateAsync(review);

        return Ok(new ApiSuccessResult<object>(new { helpfulVotes = review.HelpfulVotes }));
    }

    [HttpGet("{reviewId:guid}/replies")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<object>>> GetReviewReplies(
        [Required] Guid reviewId,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10)
    {
        var review = await _repository.GetReview(reviewId);
        if (review == null)
            return NotFound(new ApiErrorResult<object>($"Review with ID {reviewId} not found"));

        var (replies, totalCount) = await _repository.GetReviewRepliesAsync(reviewId, page, size);
        var replyDtos = replies.Adapt<List<ProductReviewDto>>();

        var result = new
        {
            content = replyDtos,
            page,
            size,
            totalElements = totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)size)
        };

        return Ok(new ApiSuccessResult<object>(result));
    }

    [HttpPost("{reviewId:guid}/replies")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    [ProducesResponseType(typeof(ApiResult<ProductReviewDto>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<ProductReviewDto>>> CreateReviewReply(
        [Required] Guid reviewId,
        [FromBody] CreateProductReviewDto replyDto)
    {
        // Reply functionality temporarily disabled - ParentReviewId not in database schema
        return BadRequest(new ApiErrorResult<ProductReviewDto>("Reply functionality is not available"));
    }
}