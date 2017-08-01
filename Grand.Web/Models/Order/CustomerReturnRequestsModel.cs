using System;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Order
{
    public partial class CustomerReturnRequestsModel : BaseGrandModel
    {
        public CustomerReturnRequestsModel()
        {
            Items = new List<ReturnRequestModel>();
        }

        public IList<ReturnRequestModel> Items { get; set; }

        #region Nested classes
        public partial class ReturnRequestModel : BaseGrandEntityModel
        {
            public int ReturnNumber { get; set; }
            public string ReturnRequestStatus { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public int Quantity { get; set; }

            public string ReturnReason { get; set; }
            public string ReturnAction { get; set; }
            public string Comments { get; set; }

            public DateTime CreatedOn { get; set; }
        }
        #endregion
    }
}