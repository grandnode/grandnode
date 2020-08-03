using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ProductAttributeMappingProfile : Profile, IMapperProfile
    {
        public ProductAttributeMappingProfile()
        {
            CreateMap<ProductAttributeMappingDto, ProductAttributeMapping>();
            CreateMap<ProductAttributeMapping, ProductAttributeMappingDto>();

            CreateMap<ProductAttributeValueDto, ProductAttributeValue>();
            CreateMap<ProductAttributeValue, ProductAttributeValueDto>();
        }

        public int Order => 1;
    }
}
