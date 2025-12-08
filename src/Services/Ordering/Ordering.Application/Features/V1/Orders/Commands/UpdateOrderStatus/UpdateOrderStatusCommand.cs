using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class UpdateOrderStatusCommand : IRequest<ApiResult<OrderDto>>
{
    public long Id { get; private set; }
    public string Status { get; set; } = string.Empty;
    
    public void SetId(long id) => Id = id;
}
