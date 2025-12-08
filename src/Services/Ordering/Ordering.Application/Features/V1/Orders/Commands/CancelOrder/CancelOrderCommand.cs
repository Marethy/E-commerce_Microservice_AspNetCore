using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class CancelOrderCommand : IRequest<ApiResult<OrderDto>>
{
    public long Id { get; private set; }
    public string? Reason { get; set; }
    
    public void SetId(long id) => Id = id;
}
