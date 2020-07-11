using DotLiquid;
using System.Collections.Generic;

namespace Grand.Domain.Messages
{
    /// <summary>
    /// An object that acumulates all DotLiquid Drops
    /// </summary>
    public partial class LiquidObject
    {
        public LiquidObject()
        {
            AdditionalTokens = new Dictionary<string, string>();
        }

        public Drop AttributeCombination { get; set; }

        public Drop Auctions { get; set; }

        public Drop BackInStockSubscription { get; set; }

        public Drop BlogComment { get; set; }

        public Drop Customer { get; set; }

        public Drop Forums { get; set; }

        public Drop GiftCard { get; set; }

        public Drop Knowledgebase { get; set; }

        public Drop NewsComment { get; set; }

        public Drop NewsLetterSubscription { get; set; }

        public Drop Order { get; set; }

        public Drop PrivateMessage { get; set; }

        public Drop Product { get; set; }

        public Drop ProductReview { get; set; }

        public Drop RecurringPayment { get; set; }

        public Drop ReturnRequest { get; set; }

        public Drop Shipment { get; set; }

        public Drop ShoppingCart { get; set; }

        public Drop Store { get; set; }

        public Drop Vendor { get; set; }

        public Drop VendorReview { get; set; }

        public Drop EmailAFriend { get; set; }

        public Drop AskQuestion { get; set; }

        public Drop VatValidationResult { get; set; }

        public Drop ContactUs { get; set; }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}