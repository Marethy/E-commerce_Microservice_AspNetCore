using AutoMapper;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Repositories.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace Product.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandRepository _repository;
        private readonly IMapper _mapper;

        public BrandsController(IBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _repository.GetBrandsAsync();
            var result = _mapper.Map<List<BrandDto>>(brands);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        public async Task<IActionResult> GetBrandById([Required] Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new { error = $"Brand with ID {id} not found." });

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(result);
        }

        [HttpGet("by-slug/{slug}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        public async Task<IActionResult> GetBrandBySlug([Required] string slug)
        {
            var brand = await _repository.GetBrandBySlugAsync(slug);
            if (brand == null)
                return NotFound(new { error = $"Brand with slug '{slug}' not found." });

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(result);
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandDto brandDto)
        {
            // Check if brand name already exists
            var existingBrand = await _repository.GetBrandByNameAsync(brandDto.Name);
            if (existingBrand != null)
                return Conflict(new { error = $"Brand name '{brandDto.Name}' already exists." });

            // Check if slug already exists
            var existingSlug = await _repository.GetBrandBySlugAsync(brandDto.Slug);
            if (existingSlug != null)
                return Conflict(new { error = $"Brand slug '{brandDto.Slug}' already exists." });

            var brand = _mapper.Map<Entities.Brand>(brandDto);
            var brandId = await _repository.CreateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return CreatedAtAction(nameof(GetBrandById), new { id = brandId }, result);
        }

        [HttpPut("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
        public async Task<IActionResult> UpdateBrand([Required] Guid id, [FromBody] CreateBrandDto brandDto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new { error = $"Brand with ID {id} not found." });

            // Check if new name conflicts with existing brand
            if (brand.Name != brandDto.Name)
            {
                var existingBrand = await _repository.GetBrandByNameAsync(brandDto.Name);
                if (existingBrand != null && existingBrand.Id != id)
                    return Conflict(new { error = $"Brand name '{brandDto.Name}' already exists." });
            }

            // Check if new slug conflicts with existing brand
            if (brand.Slug != brandDto.Slug)
            {
                var existingSlug = await _repository.GetBrandBySlugAsync(brandDto.Slug);
                if (existingSlug != null && existingSlug.Id != id)
                    return Conflict(new { error = $"Brand slug '{brandDto.Slug}' already exists." });
            }

            _mapper.Map(brandDto, brand);
            await _repository.UpdateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteBrand([Required] Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new { error = $"Brand with ID {id} not found." });

            await _repository.DeleteAsync(brand);
            return NoContent();
        }
    }
}
