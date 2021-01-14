using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Grand.Admin.Models.Orders
{
    public partial class GiftCardModel : BaseEntityModel
    {
        public GiftCardModel()
        {
            AvailableCurrencies = new List<SelectListItem>();
        }
        [GrandResourceDisplayName("Admin.GiftCards.Fields.GiftCardType")]
        public int GiftCardTypeId { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.Order")]
        public string PurchasedWithOrderId { get; set; }
        public int PurchasedWithOrderNumber { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.Amount")]
        public decimal Amount { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.CurrencyCode")]
        public string CurrencyCode { get; set; }
        public IList<SelectListItem> AvailableCurrencies { get; set; }


        [GrandResourceDisplayName("Admin.GiftCards.Fields.Amount")]
        public string AmountStr { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.RemainingAmount")]
        public string RemainingAmountStr { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.IsGiftCardActivated")]
        public bool IsGiftCardActivated { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.GiftCardCouponCode")]

        public string GiftCardCouponCode { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.RecipientName")]

        public string RecipientName { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.RecipientEmail")]

        public string RecipientEmail { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.SenderName")]

        public string SenderName { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.SenderEmail")]

        public string SenderEmail { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.Message")]

        public string Message { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.IsRecipientNotified")]
        public bool IsRecipientNotified { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        #region Nested classes

        public partial class GiftCardUsageHistoryModel : BaseEntityModel
        {
            [GrandResourceDisplayName("Admin.GiftCards.History.UsedValue")]
            public string UsedValue { get; set; }

            [GrandResourceDisplayName("Admin.GiftCards.History.Order")]
            public string OrderId { get; set; }
            public int OrderNumber { get; set; }

            [GrandResourceDisplayName("Admin.GiftCards.History.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}