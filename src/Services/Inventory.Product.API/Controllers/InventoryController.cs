using Inventory.Product.API.Services;
using Inventory.Product.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Inventory;

namespace Inventory.Product.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryController(ILogger<InventoryController> _logger, IInventoryService _inventoryService) : ControllerBase
{
    /// <summary>
    /// Get all inventory entries by item number
    /// </summary>
    [HttpGet("items/{itemNo}", Name = "GetAllByItemNo")]
    [ProducesResponseType(typeof(IEnumerable<InventoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InventoryEntryDto>>> GetAllByItemNo(string itemNo)
    {
        var result = await _inventoryService.GetAllByItemNoAsync(itemNo);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated inventory entries by item number
    /// </summary>
    [HttpGet("items/{itemNo}/paging", Name = "GetAllByItemNoPaging")]
    [ProducesResponseType(typeof(IEnumerable<InventoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<InventoryEntryDto>>> GetAllByItemNoPaging(string itemNo, [FromQuery] GetInventoryPagingQuery query)
    {
        query.SetItemNo(itemNo);
        var result = await _inventoryService.GetAllByItemNoPagingAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory entry by ID
    /// </summary>
    [HttpGet("{id}", Name = "GetInventoryById")]
    public async Task<ActionResult<InventoryEntryDto>> GetInventoryById(string id)
    {
        var result = await _inventoryService.GetInventoryByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Purchase item into inventory
    /// </summary>
    [HttpPost("purchase/{itemNo}", Name = "PurchaseOrder")]
    public async Task<ActionResult<InventoryEntryDto>> PurchaseOrder(string itemNo, PurchaseProductDto model)
    {
        var result = await _inventoryService.PurchaseItemAsync(itemNo, model);
        return Ok(result);
    }

    /// <summary>
    /// Delete inventory entry by ID
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteInventoryById")]
    public async Task<ActionResult> DeleteInventoryById(string id)
    {
        var entity = await _inventoryService.GetInventoryByIdAsync(id);
        if (entity == null) return NotFound();

        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Sales item from inventory
    /// </summary>
    [HttpPost("sales/{itemNo}", Name = "SalesItem")]
    public async Task<ActionResult<InventoryEntryDto>> SalesItem(string itemNo, SalesProductDto model)
    {
        var result = await _inventoryService.SalesItemAsync(itemNo, model);
        return Ok(result);
    }

    /// <summary>
    /// Create sales order by order number
    /// </summary>
    [HttpPost("sales/order-no/{orderNo}", Name = "SalesOrder")]
    public async Task<ActionResult<InventoryEntryDto>> SalesOrder(string orderNo, SalesOrderDto model)
    {
        model.OrderNo = orderNo;
        var documentNo = await _inventoryService.SalesOrderAsync(model);
        var result = new CreatedSalesOrderSuccessDto(documentNo);
        return Ok(result);
    }

    /// <summary>
    /// Delete inventory by document number
    /// </summary>
    [HttpDelete("document-no/{documentNo}", Name = "DeleteByDocumentNo")]
    public async Task<IActionResult> DeleteByDocumentNo(string documentNo)
    {
        await _inventoryService.DeleteByDocumentNoAsync(documentNo);
        return NoContent();
    }
}
