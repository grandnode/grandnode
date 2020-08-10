using AutoMapper;
using Grand.Domain.Orders;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class OrderSettingsProfile : Profile, IMapperProfile
    {
        public OrderSettingsProfile()
        {
            CreateMap<OrderSettings, OrderSettingsModel>()
                .ForMember(dest => dest.GiftCards_Activated_OrderStatuses, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.OrderIdent, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.IsReOrderAllowed_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinOrderSubtotalAmount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinOrderSubtotalAmountIncludingTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AnonymousCheckoutAllowed_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TermsOfServiceOnShoppingCartPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TermsOfServiceOnOrderConfirmPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.OnePageCheckoutEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnRequestsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfDaysReturnRequestAvailable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisableBillingAddressCheckoutStep_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisableOrderCompletedPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.UserCanCancelUnpaidOrder_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomerToAddOrderNote_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<OrderSettingsModel, OrderSettings>()
                .ForMember(dest => dest.MinimumOrderPlacementInterval, mo => mo.Ignore())
                .ForMember(dest => dest.UnpublishAuctionProduct, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}