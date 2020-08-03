using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class DeliveryDateProfile : Profile, IMapperProfile
    {
        public DeliveryDateProfile()
        {
            CreateMap<DeliveryDate, DeliveryDateModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<DeliveryDateModel, DeliveryDate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}