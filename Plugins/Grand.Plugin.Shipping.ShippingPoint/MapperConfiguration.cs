using Grand.Core.Infrastructure.Mapper;
using AutoMapper;
using Grand.Plugin.Shipping.ShippingPoint.Domain;
using Grand.Plugin.Shipping.ShippingPoint.Models;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class MapperConfiguration : Profile, IMapperProfile
    {
        public int Order
        {
           get { return 0; }
        }

        public MapperConfiguration()
        {
            CreateMap<ShippingPoints, ShippingPointModel>()
            .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
            .ForMember(dest => dest.StoreName, mo => mo.Ignore())
            .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
            .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
            .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            CreateMap<ShippingPointModel, ShippingPoints>();                
        }
    }
}
