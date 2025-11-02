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
using System.ComponentModel.DataAnnotations;

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

    [HttpGet("product/{productId:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetReviewsByProduct([Required] Guid productId)
    {
        var reviews = await _repository.GetReviewsByProduct(productId);
        var result = _mapper.Map<List<ProductReviewDto>>(reviews);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetReviewsByUser([Required] string userId)
    {
        var reviews = await _repository.GetReviewsByUser(userId);
        var result = _mapper.Map<List<ProductReviewDto>>(reviews);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetReviewById([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
            return NotFound(new { error = $"Review with ID {id} not found." });

        var result = _mapper.Map<ProductReviewDto>(review);
        return Ok(result);
    }

    [HttpGet("product/{productId:guid}/statistics")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetProductReviewStatistics([Required] Guid productId)
    {
        var averageRating = await _repository.GetAverageRatingByProduct(productId);
        var reviewCount = await _repository.GetReviewCountByProduct(productId);

        var result = new
        {
            ProductId = productId,
            AverageRating = Math.Round(averageRating, 2),
            ReviewCount = reviewCount
        };

        return Ok(result);
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    public async Task<IActionResult> CreateReview([FromBody] CreateProductReviewDto reviewDto)
    {
        // Verify product exists
        var product = await _productRepository.GetProduct(reviewDto.ProductId);
        if (product == null)
            return BadRequest(new { error = $"Product with ID {reviewDto.ProductId} not found." });

        // Check if user already reviewed this product
        var hasReviewed = await _repository.HasUserReviewedProduct(reviewDto.UserId, reviewDto.ProductId);
        if (hasReviewed)
            return Conflict(new { error = "User has already reviewed this product." });

        var review = _mapper.Map<ProductReview>(reviewDto);
        var reviewId = await _repository.CreateAsync(review);

        // Update product rating statistics
        await _statsService.UpdateProductRatingAsync(reviewDto.ProductId);

        var result = _mapper.Map<ProductReviewDto>(review);
        return CreatedAtAction(nameof(GetReviewById), new { id = reviewId }, result);
    }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    public async Task<IActionResult> UpdateReview([Required] Guid id, [FromBody] UpdateProductReviewDto reviewDto)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
            return NotFound(new { error = $"Review with ID {id} not found." });

        var productId = review.ProductId;

        _mapper.Map(reviewDto, review);
        await _repository.UpdateAsync(review);

        // Update product rating statistics
        await _statsService.UpdateProductRatingAsync(productId);

        var result = _mapper.Map<ProductReviewDto>(review);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    public async Task<IActionResult> DeleteReview([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
            return NotFound(new { error = $"Review with ID {id} not found." });

        var productId = review.ProductId;

        await _repository.DeleteAsync(review);

        // Update product rating statistics
        await _statsService.UpdateProductRatingAsync(productId);

        return NoContent();
    }

    [HttpPost("{id:guid}/helpful")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    public async Task<IActionResult> MarkReviewAsHelpful([Required] Guid id)
    {
        var review = await _repository.GetReview(id);
        if (review == null)
            return NotFound(new { error = $"Review with ID {id} not found." });

        review.HelpfulVotes++;
        await _repository.UpdateAsync(review);

        return Ok(new { helpfulVotes = review.HelpfulVotes });
    }
}