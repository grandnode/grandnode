using Grand.Core.Domain.Orders;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Order
{
    public class ReturnRequestDetailsModel : BaseGrandModel
    {
        public ReturnRequestDetailsModel()
        {
            ReturnRequestItems = new List<ReturnRequestItemModel>();
            PickupAddress = new AddressModel();
        }

        public IList<ReturnRequestItemModel> ReturnRequestItems { get; set; }

        public string Comments { get; set; }

        public int ReturnNumber { get; set; }

        public ReturnRequestStatus ReturnRequestStatus { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public bool ShowPickupDate { get; set; }

        public bool ShowPickupAddress { get; set; }

        public AddressModel PickupAddress { get; set; }

        public DateTime PickupDate { get; set; }

        public class ReturnRequestItemModel : BaseGrandModel
        {
            public string OrderItemId { get; set; }

            public string ReasonForReturn { get; set; }

            public int Quantity { get; set; }

            public string RequestedAction { get; set; }

            public string ProductSeName { get; set; }

            public string ProductName { get; set; }

            public string ProductPrice { get; set; }
        }
    }
}
