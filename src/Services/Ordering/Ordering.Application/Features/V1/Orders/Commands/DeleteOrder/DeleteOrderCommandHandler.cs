using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Contracts.Persistence;
using Shared.SeedWork;
using System.Threading;
using System.Threading.Tasks;

namespace Ordering.Application.Features.V1.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, ApiResult<long>>
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ApiResult<long>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetOrderAsync(request.Id);
            if (order == null)
            {
                return ApiResult<long>.Failure("Order not found.");
            }

            await _orderRepository.DeleteOrderAsync(request.Id);
            await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResult<long>.Success(request.Id);
        }
    }
}
