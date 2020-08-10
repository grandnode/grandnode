﻿using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class OrderSettingsModel : BaseGrandModel
    {
        public OrderSettingsModel()
        {
            GiftCards_Activated_OrderStatuses = new List<SelectListItem>();
        }

        public string ActiveStoreScopeConfiguration { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.IsReOrderAllowed")]
        public bool IsReOrderAllowed { get; set; }
        public bool IsReOrderAllowed_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount")]
        public decimal MinOrderSubtotalAmount { get; set; }
        public bool MinOrderSubtotalAmount_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.MinOrderSubtotalAmountIncludingTax")]
        public bool MinOrderSubtotalAmountIncludingTax { get; set; }
        public bool MinOrderSubtotalAmountIncludingTax_OverrideForStore { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.AnonymousCheckoutAllowed")]
        public bool AnonymousCheckoutAllowed { get; set; }
        public bool AnonymousCheckoutAllowed_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.TermsOfServiceOnShoppingCartPage")]
        public bool TermsOfServiceOnShoppingCartPage { get; set; }
        public bool TermsOfServiceOnShoppingCartPage_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.TermsOfServiceOnOrderConfirmPage")]
        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public bool TermsOfServiceOnOrderConfirmPage_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.OnePageCheckoutEnabled")]
        public bool OnePageCheckoutEnabled { get; set; }
        public bool OnePageCheckoutEnabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab")]
        public bool OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab { get; set; }
        public bool OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.DisableBillingAddressCheckoutStep")]
        public bool DisableBillingAddressCheckoutStep { get; set; }
        public bool DisableBillingAddressCheckoutStep_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.DisableOrderCompletedPage")]
        public bool DisableOrderCompletedPage { get; set; }
        public bool DisableOrderCompletedPage_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.AttachPdfInvoiceToOrderPlacedEmail")]
        public bool AttachPdfInvoiceToOrderPlacedEmail { get; set; }
        public bool AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.AttachPdfInvoiceToOrderPaidEmail")]
        public bool AttachPdfInvoiceToOrderPaidEmail { get; set; }
        public bool AttachPdfInvoiceToOrderPaidEmail_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.AttachPdfInvoiceToOrderCompletedEmail")]
        public bool AttachPdfInvoiceToOrderCompletedEmail { get; set; }
        public bool AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequestsEnabled")]
        public bool ReturnRequestsEnabled { get; set; }
        public bool ReturnRequestsEnabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequests_AllowToSpecifyPickupAddress")]
        public bool ReturnRequests_AllowToSpecifyPickupAddress { get; set; }
        public bool ReturnRequests_AllowToSpecifyPickupAddress_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.ReturnRequests_AllowToSpecifyPickupDate")]
        public bool ReturnRequests_AllowToSpecifyPickupDate { get; set; }
        public bool ReturnRequests_AllowToSpecifyPickupDate_OverrideForStore { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.NumberOfDaysReturnRequestAvailable")]
        public int NumberOfDaysReturnRequestAvailable { get; set; }
        public bool NumberOfDaysReturnRequestAvailable_OverrideForStore { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.GiftCards_Activated")]
        public int GiftCards_Activated_OrderStatusId { get; set; }
        public IList<SelectListItem> GiftCards_Activated_OrderStatuses { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.DeactivateGiftCardsAfterCancelOrder")]
        public bool DeactivateGiftCardsAfterCancelOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.DeactivateGiftCardsAfterDeletingOrder")]
        public bool DeactivateGiftCardsAfterDeletingOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.CompleteOrderWhenDelivered")]
        public bool CompleteOrderWhenDelivered { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.OrderIdent")]
        public int? OrderIdent { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.UserCanCancelUnpaidOrder")]
        public bool UserCanCancelUnpaidOrder { get; set; }
        public bool UserCanCancelUnpaidOrder_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Order.AllowCustomerToAddOrderNote")]
        public bool AllowCustomerToAddOrderNote { get; set; }
        public bool AllowCustomerToAddOrderNote_OverrideForStore { get; set; }
    }
}