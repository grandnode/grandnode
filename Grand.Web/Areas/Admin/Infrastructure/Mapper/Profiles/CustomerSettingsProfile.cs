using AutoMapper;
using Grand.Domain.Customers;
using Grand.Core.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerSettingsProfile : Profile, IAutoMapperProfile
    {
        public CustomerSettingsProfile()
        {
            CreateMap<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>()
                .ForMember(dest => dest.HashedPasswordFormat, mo => mo.Ignore())
                .ForMember(dest => dest.DownloadableProductsValidateUser, mo => mo.Ignore())
                .ForMember(dest => dest.OnlineCustomerMinutes, mo => mo.Ignore())
                .ForMember(dest => dest.SuffixDeletedCustomers, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}