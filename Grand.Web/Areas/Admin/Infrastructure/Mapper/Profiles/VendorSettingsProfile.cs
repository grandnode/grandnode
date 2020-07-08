using AutoMapper;
using Grand.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class VendorSettingsProfile : Profile, IMapperProfile
    {
        public VendorSettingsProfile()
        {
            CreateMap<VendorSettings, VendorSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.VendorsBlockItemsToDisplay_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowVendorOnProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToContactVendors_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToApplyForVendorAccount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowSearchByVendor_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowVendorsToEditInfo_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TermsOfServiceEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AddressSettings, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<VendorSettingsModel, VendorSettings>()
                .ForMember(dest => dest.DefaultVendorPageSizeOptions, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}