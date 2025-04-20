using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders.Queries.GetOrders
{
    public class GetOrderQuery : IRequest<ApiResult<List<OrderDto>>>
    {
        public string UserName { get; }

        public GetOrderQuery(string userName)
        {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }
    }
}