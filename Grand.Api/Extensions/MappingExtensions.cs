using Grand.Api.DTOs.Catalog;
using Grand.Api.DTOs.Customers;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
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

        #region Category
        public static CategoryDto ToModel(this Category entity)
        {
            return entity.MapTo<Category, CategoryDto>();
        }

        public static Category ToEntity(this CategoryDto model)
        {
            return model.MapTo<CategoryDto, Category>();
        }

        public static Category ToEntity(this CategoryDto model, Category destination)
        {
            return model.MapTo(destination);
        }
        #endregion

        #region Manufacturer
        public static ManufacturerDto ToModel(this Manufacturer entity)
        {
            return entity.MapTo<Manufacturer, ManufacturerDto>();
        }

        public static Manufacturer ToEntity(this ManufacturerDto model)
        {
            return model.MapTo<ManufacturerDto, Manufacturer>();
        }

        public static Manufacturer ToEntity(this ManufacturerDto model, Manufacturer destination)
        {
            return model.MapTo(destination);
        }

        #endregion

        #region Customer Role
        public static CustomerRoleDto ToModel(this CustomerRole entity)
        {
            return entity.MapTo<CustomerRole, CustomerRoleDto>();
        }

        public static CustomerRole ToEntity(this CustomerRoleDto model)
        {
            return model.MapTo<CustomerRoleDto, CustomerRole>();
        }

        public static CustomerRole ToEntity(this CustomerRoleDto model, CustomerRole destination)
        {
            return model.MapTo(destination);
        }

        #endregion
    }
}
