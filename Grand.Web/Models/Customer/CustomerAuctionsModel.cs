using Grand.Core.Domain.Catalog;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Customer
{
    public class CustomerAuctionsModel : BaseGrandModel
    {
        public CustomerAuctionsModel()
        {
            this.ProductBidList = new List<ProductBidTuple>();
        }

        public List<ProductBidTuple> ProductBidList { get; set; }
        public string CustomerId { get; set; }
    }

    public class ProductBidTuple
    {
        public Product product { get; set; }
        public Bid bid { get; set; }
    }
}