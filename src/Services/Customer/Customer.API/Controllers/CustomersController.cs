using AutoMapper;
using Customer.API.Persistence;
using Customer.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Customer;
using Shared.SeedWork.ApiResult;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Customer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    private readonly CustomerContext _context;

    public CustomersController(ICustomerRepository repository, IMapper mapper, CustomerContext context)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Get user info from JWT token (auto-creates customer if not exists)
    /// </summary>
    /// <remarks>
    /// Parses the Authorization header token to extract user identity and returns customer profile.
    /// If customer doesn't exist, it will be created automatically.
    /// </remarks>
    [HttpPost("user-info")]
    [ProducesResponseType(typeof(ApiResult<CustomerProfileDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<ActionResult<ApiResult<CustomerProfileDto>>> GetUserInfo()
    {
        try
        {
            // Get token from Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new ApiErrorResult<CustomerProfileDto>("Missing or invalid Authorization header"));
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Decode JWT token without validation (backend already validated)
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return Unauthorized(new ApiErrorResult<CustomerProfileDto>("Invalid token format"));
            }

            var jwtToken = handler.ReadJwtToken(token);
            
            // Extract claims
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
            {
                return Unauthorized(new ApiErrorResult<CustomerProfileDto>("Unable to extract user identity from token"));
            }

            // Try to find customer by username first, then by email
            var customer = !string.IsNullOrEmpty(username) 
                ? await _repository.GetCustomerByUserNameAsync(username)
                : null;

            if (customer == null && !string.IsNullOrEmpty(email))
            {
                customer = await _repository.GetCustomerByEmailAsync(email);
            }

            // Auto-create customer if not exists
            if (customer == null)
            {
                var nameParts = (name ?? email?.Split('@')[0] ?? username ?? "User").Split(' ', 2);
                customer = new Entities.Customer
                {
                    UserName = username ?? email ?? "user_" + Guid.NewGuid().ToString("N")[..8],
                    Email = email ?? $"{username}@local.com",
                    FirstName = nameParts[0],
                    LastName = nameParts.Length > 1 ? nameParts[1] : ""
                };

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
            }

            var result = _mapper.Map<CustomerProfileDto>(customer);
            return Ok(new ApiSuccessResult<CustomerProfileDto>(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiErrorResult<CustomerProfileDto>($"Error processing token: {ex.Message}"));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<CustomerDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<CustomerDto>>>> GetCustomers()
    {
    var customers = await _repository.GetCustomersAsync();
      var result = _mapper.Map<List<CustomerDto>>(customers);
        return Ok(new ApiSuccessResult<List<CustomerDto>>(result));
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ApiResult<CustomerDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<CustomerDto>>> GetCustomerByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(new ApiErrorResult<CustomerDto>("Username is required"));

        var customer = await _repository.GetCustomerByUserNameAsync(username);
        if (customer == null)
  return NotFound(new ApiErrorResult<CustomerDto>($"Customer with username '{username}' not found"));

        var result = _mapper.Map<CustomerDto>(customer);
      return Ok(new ApiSuccessResult<CustomerDto>(result));
    }

 [HttpGet("by-id/{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CustomerDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<CustomerDto>>> GetCustomerById(int id)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<CustomerDto>("Invalid customer ID"));

    var customer = await _repository.GetByIdAsync(id);
   if (customer == null)
  return NotFound(new ApiErrorResult<CustomerDto>($"Customer with ID {id} not found"));

        var result = _mapper.Map<CustomerDto>(customer);
        return Ok(new ApiSuccessResult<CustomerDto>(result));
    }
}
