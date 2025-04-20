using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResult<long>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResult<long>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("BEGIN: {HandlerName} - Creating order for Username: {UserName}",
                nameof(CreateOrderCommandHandler), request.UserName);

            try
            {
                var order = _mapper.Map<Order>(request);
                order.CreatedDate = DateTime.UtcNow;

                _orderRepository.CreateOrder(order);
                order.AddedOrder();
                await _orderRepository.SaveChangesAsync();

                _logger.LogInformation("END: {HandlerName} - Order created successfully with ID: {OrderId}",
                    nameof(CreateOrderCommandHandler), order.Id);

                return new ApiSuccessResult<long>(order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR: {HandlerName} - Failed to create order for Username: {UserName}",
                    nameof(CreateOrderCommandHandler), request.UserName);
                return new ApiErrorResult<long>("Failed to create order due to an unexpected error.");
            }
        }
    }
}