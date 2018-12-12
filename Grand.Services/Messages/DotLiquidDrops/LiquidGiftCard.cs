using DotLiquid;
using Grand.Core.Domain.Orders;
using Grand.Core.Html;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidGiftCard : Drop
    {
        private GiftCard _giftCard;

        private readonly IPriceFormatter _priceFormatter;

        public LiquidGiftCard(GiftCard giftCard)
        {
            this._priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();

            this._giftCard = giftCard;

            AdditionalTokens = new Dictionary<string, string>();
        }

        public string SenderName
        {
            get { return _giftCard.SenderName; }
        }

        public string SenderEmail
        {
            get { return _giftCard.SenderEmail; }
        }

        public string RecipientName
        {
            get { return _giftCard.RecipientName; }
        }

        public string RecipientEmail
        {
            get { return _giftCard.RecipientEmail; }
        }

        public string Amount
        {
            get { return _priceFormatter.FormatPrice(_giftCard.Amount, true, false); }
        }

        public string CouponCode
        {
            get { return _giftCard.GiftCardCouponCode; }
        }

        public string Message
        {
            get
            {
                var giftCardMesage = !String.IsNullOrWhiteSpace(_giftCard.Message) ? HtmlHelper.FormatText(_giftCard.Message, false, true, false, false, false, false) : "";
                return giftCardMesage;
            }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}