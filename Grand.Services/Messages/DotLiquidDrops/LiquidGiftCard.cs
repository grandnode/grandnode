using DotLiquid;
using Grand.Domain.Orders;
using Grand.Core.Html;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidGiftCard : Drop
    {
        private GiftCard _giftCard;

        public LiquidGiftCard(GiftCard giftCard)
        {
            _giftCard = giftCard;

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

        public string Amount { get; set; }

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