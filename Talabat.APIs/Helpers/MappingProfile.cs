using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities.Basket;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Entities.Product;


namespace Talabat.APIs.Helpers
{
    public class MappingProfile : Profile
    {
       
        public MappingProfile()
        {
          
            CreateMap<Product, ProductToReturnDto>().ForMember(d=>d.Brand , O => O.MapFrom(S=> S.Brand.Name))
                .ForMember(P=>P.Category,O=>O.MapFrom(S=>S.Category.Name))
                .ForMember(P => P.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());
            CreateMap<CustomerBasketDto, CustomerBasket>();
            CreateMap<BasketItemDto, BasketItem>();
            CreateMap<Talabat.Core.Entities.Identity.Address, AddressDto>().ReverseMap();
            CreateMap<AddressDto, Talabat.Core.Entities.Order_Aggregate.Address>();
            CreateMap<Order, OrderToReturnDto>().ForMember(d => d.DeliveryMethod, O => O.MapFrom(o => o.DeliveryMethod.ShortName))
                                               .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(o => o.DeliveryMethod.Cost));
            CreateMap<OrderItem, OrderItemDto>().ForMember(d => d.ProductName, O => O.MapFrom(o => o.Product.ProductName))
                                               .ForMember(d => d.ProductId, O => O.MapFrom(o => o.Product.ProductId))
                                               .ForMember(d => d.ProductUrl, O => O.MapFrom<OrderItemPictureUrlResolver>());
                                                               
        }
    }
}
