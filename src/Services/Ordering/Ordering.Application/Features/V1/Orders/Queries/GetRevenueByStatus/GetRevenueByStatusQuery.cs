using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetRevenueByStatus;

public class GetRevenueByStatusQuery : IRequest<ApiResult<List<RevenueByStatusDto>>>
{
}
