using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;
using static System.Net.WebRequestMethods;

namespace Talabat.APIs.Helpers
{
    public class MappingProfile : Profile
    {
       
        public MappingProfile()
        {
          
            CreateMap<Product, ProductToReturnDto>().ForMember(d=>d.Brand , O => O.MapFrom(S=> S.Brand.Name))
                .ForMember(P=>P.Category,O=>O.MapFrom(S=>S.Category.Name))
                .ForMember(P => P.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());
        }
    }
}
