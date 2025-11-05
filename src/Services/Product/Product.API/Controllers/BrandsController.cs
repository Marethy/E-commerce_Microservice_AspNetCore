using AutoMapper;
using Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.API.Repositories.Interfaces;
using Shared.Common.Constants;
using Shared.DTOs.Product;
using Shared.SeedWork.ApiResult;
using System.ComponentModel.DataAnnotations;
using System.Net;

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
        [ProducesResponseType(typeof(ApiResult<List<BrandDto>>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<List<BrandDto>>>> GetBrands()
        {
            var brands = await _repository.GetBrandsAsync();
            var result = _mapper.Map<List<BrandDto>>(brands);
            return Ok(new ApiSuccessResult<List<BrandDto>>(result));
        }

        [HttpGet("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        [ProducesResponseType(typeof(ApiResult<BrandDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ApiResult<BrandDto>>> GetBrandById([Required] Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiErrorResult<BrandDto>($"Brand with ID {id} not found"));

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(new ApiSuccessResult<BrandDto>(result));
        }

        [HttpGet("by-slug/{slug}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        [ProducesResponseType(typeof(ApiResult<BrandDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ApiResult<BrandDto>>> GetBrandBySlug([Required] string slug)
        {
            var brand = await _repository.GetBrandBySlugAsync(slug);
            if (brand == null)
                return NotFound(new ApiErrorResult<BrandDto>($"Brand with slug '{slug}' not found"));

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(new ApiSuccessResult<BrandDto>(result));
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
        [ProducesResponseType(typeof(ApiResult<BrandDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<ActionResult<ApiResult<BrandDto>>> CreateBrand([FromBody] CreateBrandDto brandDto)
        {
            var existingBrand = await _repository.GetBrandByNameAsync(brandDto.Name);
            if (existingBrand != null)
                return Conflict(new ApiErrorResult<BrandDto>($"Brand name '{brandDto.Name}' already exists"));

            var existingSlug = await _repository.GetBrandBySlugAsync(brandDto.Slug);
            if (existingSlug != null)
                return Conflict(new ApiErrorResult<BrandDto>($"Brand slug '{brandDto.Slug}' already exists"));

            var brand = _mapper.Map<Entities.Brand>(brandDto);
            var brandId = await _repository.CreateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return CreatedAtAction(nameof(GetBrandById), new { id = brandId }, new ApiSuccessResult<BrandDto>(result));
        }

        [HttpPut("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
        [ProducesResponseType(typeof(ApiResult<BrandDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ApiResult<BrandDto>>> UpdateBrand([Required] Guid id, [FromBody] CreateBrandDto brandDto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiErrorResult<BrandDto>($"Brand with ID {id} not found"));

            if (brand.Name != brandDto.Name)
            {
                var existingBrand = await _repository.GetBrandByNameAsync(brandDto.Name);
                if (existingBrand != null && existingBrand.Id != id)
                    return Conflict(new ApiErrorResult<BrandDto>($"Brand name '{brandDto.Name}' already exists"));
            }

            if (brand.Slug != brandDto.Slug)
            {
                var existingSlug = await _repository.GetBrandBySlugAsync(brandDto.Slug);
                if (existingSlug != null && existingSlug.Id != id)
                    return Conflict(new ApiErrorResult<BrandDto>($"Brand slug '{brandDto.Slug}' already exists"));
            }

            _mapper.Map(brandDto, brand);
            await _repository.UpdateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(new ApiSuccessResult<BrandDto>(result));
        }

        [HttpDelete("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteBrand([Required] Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiErrorResult<object>($"Brand with ID {id} not found"));

            await _repository.DeleteAsync(brand);
            return NoContent();
        }
    }
}
