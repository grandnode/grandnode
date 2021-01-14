using AutoMapper;
using Grand.Domain.Orders;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ReturnRequestReasonProfile : Profile, IMapperProfile
    {
        public ReturnRequestReasonProfile()
        {
            CreateMap<ReturnRequestReason, ReturnRequestReasonModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<ReturnRequestReasonModel, ReturnRequestReason>()
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}