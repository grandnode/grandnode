using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class SpecificationAttributeProfile : Profile, IMapperProfile
    {
        public SpecificationAttributeProfile()
        {
            CreateMap<SpecificationAttribute, SpecificationAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<SpecificationAttributeModel, SpecificationAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.SpecificationAttributeOptions, mo => mo.Ignore());
            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfAssociatedProducts, mo => mo.Ignore());
            CreateMap<SpecificationAttributeOptionModel, SpecificationAttributeOption>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}