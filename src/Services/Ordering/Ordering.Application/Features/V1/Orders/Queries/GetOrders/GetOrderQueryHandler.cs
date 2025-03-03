using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.V1.Orders.Queries.GetOrders
{
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, ApiResult<List<OrderDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetOrderQueryHandler> _logger;

        public GetOrderQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILogger<GetOrderQueryHandler> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResult<List<OrderDto>>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"BEGIN {nameof(GetOrderQueryHandler.Handle)} - Username: {request.UserName}");

            var orders = await _orderRepository.GetOrdersByUsernameAsync(request.UserName);

            if (!orders?.Any() ?? true)
            {
                return new ApiErrorResult<List<OrderDto>>("No orders found");
            }

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            _logger.LogInformation($"END {nameof(GetOrderQueryHandler.Handle)}");

            return new ApiSuccessResult<List<OrderDto>>(orderDtos);
        }
    }
}
