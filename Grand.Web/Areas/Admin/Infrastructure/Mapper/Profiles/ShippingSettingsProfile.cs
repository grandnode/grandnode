using AutoMapper;
using Grand.Domain.Shipping;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ShippingSettingsProfile : Profile, IMapperProfile
    {
        public ShippingSettingsProfile()
        {
            CreateMap<ShippingSettings, ShippingSettingsModel>()
                .ForMember(dest => dest.ShippingOriginAddress, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.AllowPickUpInStore_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShipToSameAddress_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.UseWarehouseLocation_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXValue_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXIncludingTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EstimateShippingEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayShipmentEventsToCustomers_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayShipmentEventsToStoreOwner_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingOriginAddress_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<ShippingSettingsModel, ShippingSettings>()
                .ForMember(dest => dest.ActiveShippingRateComputationMethodSystemNames, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnValidOptionsIfThereAreAny, mo => mo.Ignore())
                .ForMember(dest => dest.UseCubeRootMethod, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}