using AutoMapper;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.V1.Orders.Queries.GetOrders
{
    internal class GetOrderQueryHandler(IOrderRepository orderRepository, IMapper mapper) : IRequestHandler<GetOrderQuery, ApiResult<List<OrderDto>>>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<ApiResult<List<OrderDto>>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var orders= await _orderRepository.GetOrdersByUserName(request.UserName);

            if (orders == null||orders.Count()==0)
            {
                return new ApiErrorResult<List<OrderDto>>("No orders found");
            }

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            return new ApiSuccessResult<List<OrderDto>>(orderDtos);
        }
    }
}
