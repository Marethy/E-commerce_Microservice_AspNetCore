using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrderStatisticsQuery : IRequest<ApiResult<OrderStatistics>>
{
}
