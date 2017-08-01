using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Order
{
    public partial class SubmitReturnRequestModel : BaseGrandModel
    {
        public SubmitReturnRequestModel()
        {
            Items = new List<OrderItemModel>();
            AvailableReturnReasons = new List<ReturnRequestReasonModel>();
            AvailableReturnActions = new List<ReturnRequestActionModel>();
        }

        public string OrderId { get; set; }
        public int OrderNumber { get; set; }

        public IList<OrderItemModel> Items { get; set; }

        [GrandResourceDisplayName("ReturnRequests.ReturnReason")]
        public string ReturnRequestReasonId { get; set; }
        public IList<ReturnRequestReasonModel> AvailableReturnReasons { get; set; }

        [GrandResourceDisplayName("ReturnRequests.ReturnAction")]
        public string ReturnRequestActionId { get; set; }
        public IList<ReturnRequestActionModel> AvailableReturnActions { get; set; }

        [GrandResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }

        #region Nested classes

        public partial class OrderItemModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }
        }

        public partial class ReturnRequestReasonModel : BaseGrandEntityModel
        {
            public string Name { get; set; }
        }
        public partial class ReturnRequestActionModel : BaseGrandEntityModel
        {
            public string Name { get; set; }
        }

        #endregion
    }

}