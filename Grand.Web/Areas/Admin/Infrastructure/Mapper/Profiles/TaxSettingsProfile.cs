using AutoMapper;
using Grand.Domain.Tax;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class TaxSettingsProfile : Profile, IMapperProfile
    {
        public TaxSettingsProfile()
        {
            CreateMap<TaxSettings, TaxSettingsModel>()
                .ForMember(dest => dest.DefaultTaxAddress, mo => mo.Ignore())
                .ForMember(dest => dest.TaxDisplayTypeValues, mo => mo.Ignore())
                .ForMember(dest => dest.TaxBasedOnValues, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.TaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatShopCountries, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.PricesIncludeTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToSelectTaxDisplayType_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TaxDisplayType_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTaxSuffix_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTaxRates_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.HideZeroTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.HideTaxInOrderSummary_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ForceTaxExclusionFromOrderSubtotal_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TaxBasedOn_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultTaxAddress_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingIsTaxable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingPriceIncludesTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingTaxClassId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatShopCountryId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatAllowVatExemption_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatUseWebService_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatAssumeValid_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<TaxSettingsModel, TaxSettings>()
                .ForMember(dest => dest.ActiveTaxProviderSystemName, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}