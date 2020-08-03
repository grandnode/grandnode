using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Payments;
using Grand.Web.Areas.Admin.Models.Payments;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class IPaymentMethodProfile : Profile, IMapperProfile
    {
        public IPaymentMethodProfile()
        {
            CreateMap<IPaymentMethod, PaymentMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.RecurringPaymentType, mo => mo.MapFrom(src => src.RecurringPaymentType.ToString()))
                .ForMember(dest => dest.SupportCapture, mo => mo.Ignore())
                .ForMember(dest => dest.SupportPartiallyRefund, mo => mo.Ignore())
                .ForMember(dest => dest.SupportRefund, mo => mo.Ignore())
                .ForMember(dest => dest.SupportVoid, mo => mo.Ignore())
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.LogoUrl, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}