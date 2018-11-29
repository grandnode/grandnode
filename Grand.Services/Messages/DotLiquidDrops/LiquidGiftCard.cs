using DotLiquid;
using Grand.Core.Domain.Orders;
using Grand.Core.Html;
using Grand.Services.Catalog;
using System;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidGiftCard : Drop
    {
        private GiftCard _giftCard;

        private readonly IPriceFormatter _priceFormatter;

        public LiquidGiftCard(IPriceFormatter priceFormatter)
        {
            this._priceFormatter = priceFormatter;
        }

        public void SetProperties(GiftCard giftCard)
        {
            this._giftCard = giftCard;
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
    }
}