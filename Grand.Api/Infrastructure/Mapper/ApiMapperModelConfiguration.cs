using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Api.DTOs.Customers;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ApiMapperModelConfiguration : Profile, IMapperProfile
    {

        public ApiMapperModelConfiguration()
        {
            #region Category

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.SubjectToAcl, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            #endregion

            #region Manufacturer

            CreateMap<ManufacturerDto, Manufacturer>()
                .ForMember(dest => dest.SubjectToAcl, mo => mo.Ignore())
                .ForMember(dest => dest.CustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.LimitedToStores, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<Manufacturer, ManufacturerDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            #endregion

            #region CustomerRole

            CreateMap<CustomerRoleDto, CustomerRole>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<CustomerRole, CustomerRoleDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            #endregion

            #region Product attribute 

            CreateMap<ProductAttributeDto, ProductAttribute>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<ProductAttribute, ProductAttributeDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            CreateMap<PredefinedProductAttributeValue, PredefinedProductAttributeValueDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            CreateMap<PredefinedProductAttributeValueDto, PredefinedProductAttributeValue>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            #endregion

            #region Specification attribute 

            CreateMap<SpecificationAttributeDto, SpecificationAttribute>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<SpecificationAttribute, SpecificationAttributeDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionDto>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());

            CreateMap<SpecificationAttributeOptionDto, SpecificationAttributeOption>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            #endregion

        }

        public int Order => 1;
    }
}
