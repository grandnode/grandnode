using Grand.Api.DTOs.Catalog;
using Grand.Core.Domain.Catalog;
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

        public static CategoryDTO ToModel(this Category entity)
        {
            return entity.MapTo<Category, CategoryDTO>();
        }

        public static Category ToEntity(this CategoryDTO model)
        {
            return model.MapTo<CategoryDTO, Category>();
        }

        public static Grand.Core.Domain.Catalog.Category ToEntity(this CategoryDTO model, Category destination)
        {
            return model.MapTo(destination);
        }
    }
}
