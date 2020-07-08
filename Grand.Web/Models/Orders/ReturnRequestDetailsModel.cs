using Grand.Domain.Orders;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Orders
{
    public class ReturnRequestDetailsModel : BaseGrandModel
    {
        public ReturnRequestDetailsModel()
        {
            ReturnRequestItems = new List<ReturnRequestItemModel>();
            PickupAddress = new AddressModel();
            ReturnRequestNotes = new List<ReturnRequestNote>();
        }

        public IList<ReturnRequestItemModel> ReturnRequestItems { get; set; }

        public string Comments { get; set; }

        public int ReturnNumber { get; set; }

        public string ExternalId { get; set; }

        public ReturnRequestStatus ReturnRequestStatus { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public bool ShowPickupDate { get; set; }

        public bool ShowPickupAddress { get; set; }

        public AddressModel PickupAddress { get; set; }

        public DateTime PickupDate { get; set; }

        public IList<ReturnRequestNote> ReturnRequestNotes { get; set; }

        public bool ShowAddReturnRequestNote { get; set; }


        #region Nested Classes

        public partial class ReturnRequestNote : BaseGrandEntityModel
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
            public string ReturnRequestId { get; set; }
        }

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

        #endregion
    }
}
