using Grand.Core.Infrastructure.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Grand.Plugin.Shipping.ShippingPoint.Domain;
using Grand.Plugin.Shipping.ShippingPoint.Models;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class MapperConfiguration : IMapperConfiguration
    {
        public int Order
        {
           get { return 0; }
        }

        public Action<IMapperConfigurationExpression> GetConfiguration()
        {
            Action<IMapperConfigurationExpression> action = cfg =>
            {
                cfg.CreateMap<ShippingPoints, ShippingPointModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.StoreName, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
                cfg.CreateMap<ShippingPointModel, ShippingPoints>();                
            };

            return action;
        }
    }
}
