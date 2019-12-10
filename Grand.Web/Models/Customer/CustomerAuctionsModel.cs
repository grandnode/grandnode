using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public class CustomerAuctionsModel : BaseGrandModel
    {
        public CustomerAuctionsModel()
        {
            ProductBidList = new List<ProductBidTuple>();
        }

        public List<ProductBidTuple> ProductBidList { get; set; }
        public string CustomerId { get; set; }
    }

    public class ProductBidTuple
    {
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string CurrentBidAmount { get; set; }
        public decimal CurrentBidAmountValue { get; set; }
        public string BidAmount { get; set; }
        public decimal BidAmountValue { get; set; }
        public DateTime EndBidDate { get; set; } 
        public bool Ended { get; set; }
        public bool HighestBidder { get; set; }
        public string OrderId { get; set; }
    }
}