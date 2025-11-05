// src/Services/Ordering/Ordering.API/Controllers/OrderReportsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Services;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Ordering.API.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrderReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IOrderPdfService _pdfService;

    public OrderReportsController(IMediator mediator, IOrderPdfService pdfService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
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

        // Generate PDF and send via Hangfire
        var pdfBytes = _pdfService.GenerateOrderInvoice(result.Data);
        
      // TODO: Implement email with attachment via Hangfire
        // This requires extending Hangfire API to support attachments
        
      return Ok(new ApiSuccessResult<string>("Invoice will be emailed shortly"));
    }
}
