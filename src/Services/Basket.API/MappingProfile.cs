using AutoMapper;
using Basket.API.Entities;
using EventBus.MessageComponents.Consumers.Basket;

namespace Basket.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BasketCheckout, BasketCheckoutEvent>();
        }
    }
}