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

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IBrandRepository _brandRepository;
    private readonly ISellerRepository _sellerRepository;
    private readonly IMapper _mapper;

    public ProductsController(
        IProductRepository repository,
        IBrandRepository brandRepository,
      ISellerRepository sellerRepository,
    IMapper mapper)
    {
        _repository = repository;
        _brandRepository = brandRepository;
  _sellerRepository = sellerRepository;
        _mapper = mapper;
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
        var product = await _repository.GetProduct(id);
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