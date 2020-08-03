using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ProductAttributeProfile : Profile, IMapperProfile
    {
        public ProductAttributeProfile()
        {
            CreateMap<ProductAttribute, ProductAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<ProductAttributeModel, ProductAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}