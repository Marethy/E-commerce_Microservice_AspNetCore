using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Repositories.Interfaces;
using Shared.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace Product.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductRepository repository, IMapper mapper) : ControllerBase
    {
        private readonly IProductRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _repository.GetProducts();
            var result = _mapper.Map<List<ProductDto>>(products);
            return Ok(result);
        }
        [HttpGet("get-product-by-no/{productNo}")]
        public async Task<IActionResult> GetProductByNo([Required] string productNo)
        {
            if (string.IsNullOrWhiteSpace(productNo))
            {
                return BadRequest("Product No is required.");
            }

            var product = await _repository.GetProductByNo(productNo);
            if (product == null)
            {
                return NotFound($"Product with No '{productNo}' not found.");
            }

            var result = _mapper.Map<ProductDto>(product);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetProductById([Required] long id)
        {
            var product = await _repository.GetProduct(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            var result = _mapper.Map<ProductDto>(product);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Product data is required.");
            }

            var productEntity = await _repository.GetProductByNo(productDto.No);
            if (productEntity != null)
            {
                return BadRequest(new { error = $"Product No: {productDto.No} is existed." });
            }

            var product = _mapper.Map<CatalogProduct>(productDto);
            await _repository.CreateProduct(product);
            await _repository.SaveChangesAsync();

            var result = _mapper.Map<ProductDto>(product);
            return Ok(result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateProduct([Required] long id, [FromBody] UpdateProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Product update data is required.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _repository.GetProduct(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            _mapper.Map(productDto, product);
            await _repository.SaveChangesAsync();

            var result = _mapper.Map<ProductDto>(product);
            return Ok(result);
        }


        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteProduct([Required] long id)
        {
            var product = await _repository.GetProduct(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }
            await _repository.DeleteProduct(id);
            await _repository.SaveChangesAsync();
            return NoContent();
        }
    }
}
