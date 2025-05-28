using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrderByIdQuery(long id) : IRequest<ApiResult<OrderDto>>
{
    public long Id { get; private set; } = id;
}