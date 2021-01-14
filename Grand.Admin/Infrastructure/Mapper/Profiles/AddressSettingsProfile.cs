using AutoMapper;
using Grand.Domain.Common;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class AddressSettingsProfile : Profile, IMapperProfile
    {
        public AddressSettingsProfile()
        {
            CreateMap<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>();
        }

        public int Order => 0;
    }
}