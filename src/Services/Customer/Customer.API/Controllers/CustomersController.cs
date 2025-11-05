using AutoMapper;
using Customer.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Customer;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Customer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;

public CustomersController(ICustomerRepository repository, IMapper mapper)
    {
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
  _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
