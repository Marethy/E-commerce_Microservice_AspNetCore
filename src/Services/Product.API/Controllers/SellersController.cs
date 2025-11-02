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
    public class SellersController : ControllerBase
    {
        private readonly ISellerRepository _repository;
        private readonly IMapper _mapper;

        public SellersController(ISellerRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        public async Task<IActionResult> GetSellers([FromQuery] bool? officialOnly = null)
        {
            var sellers = officialOnly == true 
                ? await _repository.GetOfficialSellersAsync()
                : await _repository.GetSellersAsync();

            var result = _mapper.Map<List<SellerDto>>(sellers);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.VIEW)]
        public async Task<IActionResult> GetSellerById([Required] Guid id)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new { error = $"Seller with ID {id} not found." });

            var result = _mapper.Map<SellerDto>(seller);
            return Ok(result);
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.CREATE)]
        public async Task<IActionResult> CreateSeller([FromBody] CreateSellerDto sellerDto)
        {
            // Check if seller name already exists
            var existingSeller = await _repository.GetSellerByNameAsync(sellerDto.Name);
            if (existingSeller != null)
                return Conflict(new { error = $"Seller name '{sellerDto.Name}' already exists." });

            var seller = _mapper.Map<Entities.Seller>(sellerDto);
            seller.Rating = 0;
            seller.TotalSales = 0;

            var sellerId = await _repository.CreateAsync(seller);

            var result = _mapper.Map<SellerDto>(seller);
            return CreatedAtAction(nameof(GetSellerById), new { id = sellerId }, result);
        }

        [HttpPut("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.UPDATE)]
        public async Task<IActionResult> UpdateSeller([Required] Guid id, [FromBody] CreateSellerDto sellerDto)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new { error = $"Seller with ID {id} not found." });

            // Check if new name conflicts with existing seller
            if (seller.Name != sellerDto.Name)
            {
                var existingSeller = await _repository.GetSellerByNameAsync(sellerDto.Name);
                if (existingSeller != null && existingSeller.Id != id)
                    return Conflict(new { error = $"Seller name '{sellerDto.Name}' already exists." });
            }

            _mapper.Map(sellerDto, seller);
            await _repository.UpdateAsync(seller);

            var result = _mapper.Map<SellerDto>(seller);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ClaimRequirement(FunctionCode.PRODUCT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteSeller([Required] Guid id)
        {
            var seller = await _repository.GetByIdAsync(id);
            if (seller == null)
                return NotFound(new { error = $"Seller with ID {id} not found." });

            await _repository.DeleteAsync(seller);
            return NoContent();
        }
    }
}
