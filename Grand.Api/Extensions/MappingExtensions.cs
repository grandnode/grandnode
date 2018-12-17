using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Extensions
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

        public static Grand.Api.Model.Catalog.Category ToModel(this Grand.Core.Domain.Catalog.Category entity)
        {
            return entity.MapTo<Grand.Core.Domain.Catalog.Category, Grand.Api.Model.Catalog.Category>();
        }

        public static Grand.Core.Domain.Catalog.Category ToEntity(this Grand.Api.Model.Catalog.Category model)
        {
            return model.MapTo<Grand.Api.Model.Catalog.Category, Grand.Core.Domain.Catalog.Category>();
        }

        public static Grand.Core.Domain.Catalog.Category ToEntity(this Grand.Api.Model.Catalog.Category model, Grand.Core.Domain.Catalog.Category destination)
        {
            return model.MapTo(destination);
        }
    }
}
