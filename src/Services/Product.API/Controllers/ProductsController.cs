using AutoMapper;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace Product.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductsController(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetProducts([FromQuery] Guid? categoryId = null)
    {
        var products = categoryId.HasValue
            ? await _repository.GetProductsByCategory(categoryId.Value)
            : await _repository.GetProducts();

        var result = _mapper.Map<List<ProductDto>>(products);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetProductById([Required] Guid id)
    {
        var product = await _repository.GetProduct(id);
        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpGet("by-no/{productNo}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetProductByNo([Required] string productNo)
    {
        var product = await _repository.GetProductByNo(productNo);
        if (product == null)
            return NotFound(new { error = $"Product No '{productNo}' not found." });

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
    {
        // Check if product No already exists
        var existingProduct = await _repository.GetProductByNo(productDto.No);
        if (existingProduct != null)
            return Conflict(new { error = $"Product No '{productDto.No}' already exists." });

        // Verify category exists
        var categoryExists = await _repository.CategoryExists(productDto.CategoryId);
        if (!categoryExists)
            return BadRequest(new { error = $"Category ID {productDto.CategoryId} not found." });

        var product = _mapper.Map<CatalogProduct>(productDto);
        var productId = await _repository.CreateAsync(product);

        var result = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetProductById), new { id = productId }, result);
    }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    public async Task<IActionResult> UpdateProduct([Required] Guid id, [FromBody] UpdateProductDto productDto)
    {
        var product = await _repository.GetProduct(id);
        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        // If updating category, verify it exists
        if (productDto.CategoryId.HasValue)
        {
            var categoryExists = await _repository.CategoryExists(productDto.CategoryId.Value);
            if (!categoryExists)
                return BadRequest(new { error = $"Category ID {productDto.CategoryId} not found." });
        }

        _mapper.Map(productDto, product);
        await _repository.UpdateAsync(product);

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    public async Task<IActionResult> DeleteProduct([Required] Guid id)
    {
        var product = await _repository.GetProduct(id);
        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        await _repository.DeleteAsync(product);
        return NoContent();
    }
}