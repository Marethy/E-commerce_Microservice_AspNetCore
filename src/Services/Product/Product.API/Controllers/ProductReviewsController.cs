using AutoMapper;
using Infrastructure.Identity.Authorization;
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
    private readonly IMapper _mapper;

    public ProductReviewsController(
        IProductReviewRepository repository,
  IProductRepository productRepository,
    IProductStatsService statsService,
   IMapper mapper)
    {
    _repository = repository;
        _productRepository = productRepository;
        _statsService = statsService;
     _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductReviewDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductReviewDto>>>> GetAllReviews()
    {
        var reviews = await _repository.GetAllReviewsAsync();
        var result = _mapper.Map<List<ProductReviewDto>>(reviews);
        return Ok(new ApiSuccessResult<List<ProductReviewDto>>(result));
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductReviewDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductReviewDto>>>> GetReviewsByProduct([Required] Guid productId)
    {
        var reviews = await _repository.GetReviewsByProduct(productId);
        var result = _mapper.Map<List<ProductReviewDto>>(reviews);
        return Ok(new ApiSuccessResult<List<ProductReviewDto>>(result));
 }

    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<List<ProductReviewDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<ProductReviewDto>>>> GetReviewsByUser([Required] string userId)
    {
        var reviews = await _repository.GetReviewsByUser(userId);
   var result = _mapper.Map<List<ProductReviewDto>>(reviews);
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

        var result = _mapper.Map<ProductReviewDto>(review);
        return Ok(new ApiSuccessResult<ProductReviewDto>(result));
    }

    [HttpGet("product/{productId:guid}/statistics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
  public async Task<ActionResult<ApiResult<object>>> GetProductReviewStatistics([Required] Guid productId)
    {
        var averageRating = await _repository.GetAverageRatingByProduct(productId);
 var reviewCount = await _repository.GetReviewCountByProduct(productId);

        var result = new
        {
          ProductId = productId,
 AverageRating = Math.Round(averageRating, 2),
        ReviewCount = reviewCount
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

        var review = _mapper.Map<ProductReview>(reviewDto);
        var reviewId = await _repository.CreateAsync(review);

        await _statsService.UpdateProductRatingAsync(reviewDto.ProductId);

        var result = _mapper.Map<ProductReviewDto>(review);
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

     _mapper.Map(reviewDto, review);
        await _repository.UpdateAsync(review);

        await _statsService.UpdateProductRatingAsync(productId);

        var result = _mapper.Map<ProductReviewDto>(review);
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
        var replyDtos = _mapper.Map<List<ProductReviewDto>>(replies);

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