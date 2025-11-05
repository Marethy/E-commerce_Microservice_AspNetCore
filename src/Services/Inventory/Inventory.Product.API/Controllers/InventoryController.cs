using Inventory.Product.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Inventory;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Inventory.Product.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;
    private readonly IInventoryService _inventoryService;

    public InventoryController(ILogger<InventoryController> logger, IInventoryService inventoryService)
    {
        _logger = logger;
      _inventoryService = inventoryService;
    }

    /// <summary>
    /// Get all inventory entries by item number
    /// </summary>
    /// <param name="itemNo"></param>
    /// <returns></returns>
    [HttpGet("items/{itemNo}", Name = "GetAllByItemNo")]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<InventoryEntryDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<IEnumerable<InventoryEntryDto>>>> GetAllByItemNo(string itemNo)
    {
   var result = await _inventoryService.GetAllByItemNoAsync(itemNo);
        return Ok(new ApiSuccessResult<IEnumerable<InventoryEntryDto>>(result));
 }

    /// <summary>
    /// Get paginated inventory entries by item number
    /// </summary>
    [HttpGet("items/{itemNo}/paging", Name = "GetAllByItemNoPaging")]
    [ProducesResponseType(typeof(ApiResult<IEnumerable<InventoryEntryDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResult<IEnumerable<InventoryEntryDto>>>> GetAllByItemNoPaging(string itemNo, [FromQuery] GetInventoryPagingQuery query)
    {
        query.SetItemNo(itemNo);
        var result = await _inventoryService.GetAllByItemNoPagingAsync(query);
        return Ok(new ApiSuccessResult<IEnumerable<InventoryEntryDto>>(result));
    }

    /// <summary>
    /// Get inventory entry by ID
    /// </summary>
    [HttpGet("{id}", Name = "GetInventoryById")]
    [ProducesResponseType(typeof(ApiResult<InventoryEntryDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<InventoryEntryDto>>> GetInventoryById(string id)
    {
        var result = await _inventoryService.GetInventoryByIdAsync(id);
   if (result == null)
  return NotFound(new ApiErrorResult<InventoryEntryDto>($"Inventory with ID {id} not found"));
    
        return Ok(new ApiSuccessResult<InventoryEntryDto>(result));
    }

    /// <summary>
    /// Purchase item into inventory
    /// </summary>
    [HttpPost("purchase/{itemNo}", Name = "PurchaseOrder")]
    [ProducesResponseType(typeof(ApiResult<InventoryEntryDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<InventoryEntryDto>>> PurchaseOrder(string itemNo, [FromBody] PurchaseProductDto model)
    {
        var result = await _inventoryService.PurchaseItemAsync(itemNo, model);
    return Ok(new ApiSuccessResult<InventoryEntryDto>(result));
    }

    /// <summary>
    /// Delete inventory entry by ID
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteInventoryById")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> DeleteInventoryById(string id)
    {
        var entity = await _inventoryService.GetInventoryByIdAsync(id);
        if (entity == null)
          return NotFound(new ApiErrorResult<object>($"Inventory with ID {id} not found"));

        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Sales item from inventory
    /// </summary>
    [HttpPost("sales/{itemNo}", Name = "SalesItem")]
    [ProducesResponseType(typeof(ApiResult<InventoryEntryDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<InventoryEntryDto>>> SalesItem(string itemNo, [FromBody] SalesProductDto model)
  {
  var result = await _inventoryService.SalesItemAsync(itemNo, model);
   return Ok(new ApiSuccessResult<InventoryEntryDto>(result));
    }

    /// <summary>
    /// Create sales order by order number
    /// </summary>
    [HttpPost("sales/order-no/{orderNo}", Name = "SalesOrder")]
    [ProducesResponseType(typeof(ApiResult<CreatedSalesOrderSuccessDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<CreatedSalesOrderSuccessDto>>> SalesOrder(string orderNo, [FromBody] SalesOrderDto model)
    {
        model.OrderNo = orderNo;
  var documentNo = await _inventoryService.SalesOrderAsync(model);
        var result = new CreatedSalesOrderSuccessDto(documentNo);
     return Ok(new ApiSuccessResult<CreatedSalesOrderSuccessDto>(result));
    }

    /// <summary>
    /// Delete inventory by document number
    /// </summary>
    [HttpDelete("document-no/{documentNo}", Name = "DeleteByDocumentNo")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<ActionResult> DeleteByDocumentNo(string documentNo)
    {
        await _inventoryService.DeleteByDocumentNoAsync(documentNo);
        return NoContent();
    }
}
