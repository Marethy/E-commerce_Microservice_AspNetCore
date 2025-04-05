using MediatR;
using Shared.SeedWork;
using System.ComponentModel.DataAnnotations;

namespace Ordering.Application.Features.V1.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommand : IRequest<ApiResult<long>>
    {
        [Required(ErrorMessage = "Order ID is required.")]
        public long Id { get; }

        public DeleteOrderCommand(long id)
        {
            Id = id <= 0 ? throw new ArgumentException("Order ID must be greater than zero", nameof(id)) : id;
        }
    }
}