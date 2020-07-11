using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ProductAttributeProfile : Profile, IMapperProfile
    {
        public ProductAttributeProfile()
        {

            CreateMap<ProductAttributeDto, ProductAttribute>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<ProductAttribute, ProductAttributeDto>();

            CreateMap<PredefinedProductAttributeValue, PredefinedProductAttributeValueDto>();

            CreateMap<PredefinedProductAttributeValueDto, PredefinedProductAttributeValue>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

        }

        public int Order => 1;
    }
}
