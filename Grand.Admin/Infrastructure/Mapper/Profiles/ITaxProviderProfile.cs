using AutoMapper;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Tax;
using Grand.Admin.Models.Tax;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ITaxProviderProfile : Profile, IMapperProfile
    {
        public ITaxProviderProfile()
        {
            CreateMap<ITaxProvider, TaxProviderModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.IsPrimaryTaxProvider, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}