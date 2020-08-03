using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class PredefinedProductAttributeValueProfile : Profile, IMapperProfile
    {
        public PredefinedProductAttributeValueProfile()
        {
            CreateMap<PredefinedProductAttributeValue, PredefinedProductAttributeValueModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.PriceAdjustmentStr, mo => mo.MapFrom(x => x.PriceAdjustment.ToString("N2")))
                .ForMember(dest => dest.WeightAdjustmentStr, mo => mo.MapFrom(x => x.WeightAdjustment.ToString("N2")));

            CreateMap<PredefinedProductAttributeValueModel, PredefinedProductAttributeValue>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}