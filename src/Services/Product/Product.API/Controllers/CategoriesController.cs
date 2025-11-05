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
    [ProducesResponseType(typeof(ApiResult<List<CategoryDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<CategoryDto>>>> GetCategories([FromQuery] bool includeProducts = false)
    {
        var categories = includeProducts 
            ? await _repository.GetCategoriesWithProducts()
            : await _repository.GetCategories();

        var result = _mapper.Map<List<CategoryDto>>(categories);
        return Ok(new ApiSuccessResult<List<CategoryDto>>(result));
    }

    [HttpGet("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<CategoryDto>>> GetCategoryById([Required] Guid id)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new ApiErrorResult<CategoryDto>($"Category with ID {id} not found"));

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(new ApiSuccessResult<CategoryDto>(result));
    }

    [HttpGet("by-name/{name}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<CategoryDto>>> GetCategoryByName([Required] string name)
    {
        var category = await _repository.GetCategoryByName(name);
        if (category == null)
            return NotFound(new ApiErrorResult<CategoryDto>($"Category '{name}' not found"));

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(new ApiSuccessResult<CategoryDto>(result));
    }

    [HttpPost]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    public async Task<ActionResult<ApiResult<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto categoryDto)
    {
        // Check if category name already exists
        var existingCategory = await _repository.GetCategoryByName(categoryDto.Name);
        if (existingCategory != null)
            return Conflict(new ApiErrorResult<CategoryDto>($"Category '{categoryDto.Name}' already exists"));

        var category = _mapper.Map<Category>(categoryDto);
        var categoryId = await _repository.CreateAsync(category);

        var result = _mapper.Map<CategoryDto>(category);
        return CreatedAtAction(nameof(GetCategoryById), new { id = categoryId }, new ApiSuccessResult<CategoryDto>(result));
    }

    [HttpPut("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
    [ProducesResponseType(typeof(ApiResult<CategoryDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<CategoryDto>>> UpdateCategory([Required] Guid id, [FromBody] UpdateCategoryDto categoryDto)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new ApiErrorResult<CategoryDto>($"Category with ID {id} not found"));

        // Check if new name conflicts with existing category
        if (!string.IsNullOrEmpty(categoryDto.Name) && categoryDto.Name != category.Name)
        {
            var existingCategory = await _repository.GetCategoryByName(categoryDto.Name);
            if (existingCategory != null)
                return Conflict(new ApiErrorResult<CategoryDto>($"Category '{categoryDto.Name}' already exists"));
        }

        _mapper.Map(categoryDto, category);
        await _repository.UpdateAsync(category);

        var result = _mapper.Map<CategoryDto>(category);
        return Ok(new ApiSuccessResult<CategoryDto>(result));
    }

    [HttpDelete("{id:guid}")]
    [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteCategory([Required] Guid id)
    {
        var category = await _repository.GetCategory(id);
        if (category == null)
            return NotFound(new ApiErrorResult<object>($"Category with ID {id} not found"));

        await _repository.DeleteAsync(category);
        return NoContent();
    }
}