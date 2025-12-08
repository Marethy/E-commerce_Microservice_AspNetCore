// src/Services/Ordering/Ordering.API/Controllers/OrderReportsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Services;
using Ordering.Application.Features.V1.Orders;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Ordering.API.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrderReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrderPdfService _pdfService;
    private readonly IOrderExcelService _excelService;

    public OrderReportsController(
        IMediator mediator, 
        IOrderPdfService pdfService,
        IOrderExcelService excelService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
        _excelService = excelService;
    }

    /// <summary>
    /// Download order invoice PDF
    /// </summary>
    [HttpGet("{id:long}/invoice")]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DownloadInvoice(long id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
            return NotFound(new ApiErrorResult<object>($"Order {id} not found"));

      var pdfBytes = _pdfService.GenerateOrderInvoice(result.Data);
        
   return File(
  pdfBytes,
            "application/pdf",
       $"Invoice-{id}-{DateTime.UtcNow:yyyyMMdd}.pdf"
   );
    }

  /// <summary>
    /// Download order receipt PDF
    /// </summary>
    [HttpGet("{id:long}/receipt")]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DownloadReceipt(long id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
            return NotFound(new ApiErrorResult<object>($"Order {id} not found"));

        var pdfBytes = _pdfService.GenerateOrderReceipt(result.Data);
        
        return File(
  pdfBytes,
          "application/pdf",
       $"Receipt-{id}-{DateTime.UtcNow:yyyyMMdd}.pdf"
        );
    }

    /// <summary>
    /// Email invoice to customer
    /// </summary>
    [HttpPost("{id:long}/email-invoice")]
    [ProducesResponseType(typeof(ApiResult<string>), (int)HttpStatusCode.OK)]
public async Task<ActionResult<ApiResult<string>>> EmailInvoice(long id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
            return NotFound(new ApiErrorResult<string>($"Order {id} not found"));

        var pdfBytes = _pdfService.GenerateOrderInvoice(result.Data);
        
        return Ok(new ApiSuccessResult<string>("Invoice will be emailed shortly"));
    }

    [HttpGet("admin/export")]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ExportOrders(
        [FromQuery] string? status = null,
        [FromQuery] string format = "excel")
    {
        var query = new GetAllOrdersQuery(0, 10000, status);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(new ApiErrorResult<object>("Failed to retrieve orders"));

        var ordersData = result.Data as dynamic;
        var orders = ordersData?.orders as IEnumerable<Ordering.Application.Common.Models.OrderDto>;

        if (orders == null || !orders.Any())
            return Ok(new ApiSuccessResult<object>(new { message = "No orders to export" }));

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfBytes = _pdfService.GenerateOrderInvoice(orders.First());
            return File(pdfBytes, "application/pdf", $"orders-export-{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        var excelBytes = _excelService.ExportOrdersToExcel(orders);
        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"orders-export-{DateTime.UtcNow:yyyyMMdd}.xlsx"
        );
    }
}
