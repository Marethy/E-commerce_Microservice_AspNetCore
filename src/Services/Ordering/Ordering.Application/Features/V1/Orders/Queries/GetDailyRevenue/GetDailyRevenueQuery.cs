using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders.Queries.GetDailyRevenue;

public class GetDailyRevenueQuery : IRequest<ApiResult<List<DailyRevenueDto>>>
{
    public int Days { get; set; } = 30;

    public GetDailyRevenueQuery(int days = 30)
    {
        Days = days;
    }
}
