using Infrastructure.Identity.Authorization;
using Mapster;
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
    public class SellersController : ControllerBase
    {
        private readonly ISellerRepository _repository;

        public SellersController(ISellerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<List<SellerDto>>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<List<SellerDto>>>> GetSellers([FromQuery] bool? officialOnly = null)
        {
            var sellers = officialOnly == true 
                ? await _repository.GetOfficialSellersAsync()
                : await _repository.GetSellersAsync();

            var result = sellers.Adapt<List<SellerDto>>();
            return Ok(new ApiSuccessResult<List<SellerDto>>(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResult<SellerDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ApiResult<SellerDto>>> GetSellerById([Required] Guid id)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new ApiErrorResult<SellerDto>($"Seller with ID {id} not found"));

            var result = seller.Adapt<SellerDto>();
            return Ok(new ApiSuccessResult<SellerDto>(result));
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
        [ProducesResponseType(typeof(ApiResult<SellerDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<ActionResult<ApiResult<SellerDto>>> CreateSeller([FromBody] CreateSellerDto sellerDto)
        {
            // Check if seller name already exists
            var existingSeller = await _repository.GetSellerByNameAsync(sellerDto.Name);
            if (existingSeller != null)
                return Conflict(new ApiErrorResult<SellerDto>($"Seller name '{sellerDto.Name}' already exists"));

            var seller = sellerDto.Adapt<Entities.Seller>();
            seller.Rating = 0;
            seller.TotalSales = 0;

            var sellerId = await _repository.CreateAsync(seller);

            var result = seller.Adapt<SellerDto>();
            return CreatedAtAction(nameof(GetSellerById), new { id = sellerId }, new ApiSuccessResult<SellerDto>(result));
        }

        [HttpPut("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
        [ProducesResponseType(typeof(ApiResult<SellerDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ApiResult<SellerDto>>> UpdateSeller([Required] Guid id, [FromBody] CreateSellerDto sellerDto)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new ApiErrorResult<SellerDto>($"Seller with ID {id} not found"));

            // Check if new name conflicts with existing seller
            if (seller.Name != sellerDto.Name)
            {
                var existingSeller = await _repository.GetSellerByNameAsync(sellerDto.Name);
                if (existingSeller != null && existingSeller.Id != id)
                    return Conflict(new ApiErrorResult<SellerDto>($"Seller name '{sellerDto.Name}' already exists"));
            }

            sellerDto.Adapt(seller);
            await _repository.UpdateAsync(seller);

            var result = seller.Adapt<SellerDto>();
            return Ok(new ApiSuccessResult<SellerDto>(result));
        }

        [HttpDelete("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteSeller([Required] Guid id)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new ApiErrorResult<object>($"Seller with ID {id} not found"));

            await _repository.DeleteAsync(seller);
            return NoContent();
        }
    }
}
