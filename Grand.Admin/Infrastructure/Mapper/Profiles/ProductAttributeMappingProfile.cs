using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Catalog;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ProductAttributeMappingProfile : Profile, IMapperProfile
    {
        public ProductAttributeMappingProfile()
        {
            CreateMap<ProductAttributeMapping, ProductModel.ProductAttributeMappingModel>();

            CreateMap<ProductModel.ProductAttributeMappingModel, ProductAttributeMapping>()
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}