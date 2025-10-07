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
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoriesController(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetCategories([FromQuery] bool includeProducts = false)
    {
        var categories = includeProducts 
            ? await _repository.GetCategoriesWithProducts()
            : await _repository.GetCategories();

        var result = _mapper.Map<List<CategoryDto>>(categories);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetCategoryById([Required] Guid id)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new { error = $"Category with ID {id} not found." });

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(result);
    }

    [HttpGet("by-name/{name}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    public async Task<IActionResult> GetCategoryByName([Required] string name)
    {
        var category = await _repository.GetCategoryByName(name);
        if (category == null)
            return NotFound(new { error = $"Category '{name}' not found." });

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(result);
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        // Check if category name already exists
        var existingCategory = await _repository.GetCategoryByName(categoryDto.Name);
        if (existingCategory != null)
            return Conflict(new { error = $"Category '{categoryDto.Name}' already exists." });

        var category = _mapper.Map<Category>(categoryDto);
        var categoryId = await _repository.CreateAsync(category);

        var result = _mapper.Map<CategoryDto>(category);
        return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, result);
    }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    public async Task<IActionResult> UpdateCategory([Required] Guid id, [FromBody] UpdateCategoryDto categoryDto)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new { error = $"Category with ID {id} not found." });

        // Check if new name conflicts with existing category
        if (!string.IsNullOrEmpty(categoryDto.Name) && categoryDto.Name != category.Name)
        {
            var existingCategory = await _repository.GetCategoryByName(categoryDto.Name);
            if (existingCategory != null)
                return Conflict(new { error = $"Category '{categoryDto.Name}' already exists." });
        }

        _mapper.Map(categoryDto, category);
        await _repository.UpdateAsync(category);

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    public async Task<IActionResult> DeleteCategory([Required] Guid id)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new { error = $"Category with ID {id} not found." });

        await _repository.DeleteAsync(category);
        return NoContent();
    }
}