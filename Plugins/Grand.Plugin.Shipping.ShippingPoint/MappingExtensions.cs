using Grand.Core.Infrastructure.Mapper;
using Grand.Plugin.Shipping.ShippingPoint.Domain;
using Grand.Plugin.Shipping.ShippingPoint.Models;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public static class MappingExtensions
    {
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            return AutoMapperConfiguration.Mapper.Map<TSource, TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return AutoMapperConfiguration.Mapper.Map(source, destination);
        }

        public static ShippingPointModel ToModel(this ShippingPoints entity)
        {
            return entity.MapTo<ShippingPoints, ShippingPointModel>();
        }

        public static ShippingPoints ToEntity(this ShippingPointModel model)
        {
            return model.MapTo<ShippingPointModel, ShippingPoints>();
        }

        public static ShippingPoints ToEntity(this ShippingPointModel model, ShippingPoints destination)
        {
            return model.MapTo(destination);
        }

    }
}
