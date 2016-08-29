﻿using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Order
{
    public partial class SubmitReturnRequestModel : BaseNopModel
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

        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.ReturnReason")]
        public string ReturnRequestReasonId { get; set; }
        public IList<ReturnRequestReasonModel> AvailableReturnReasons { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.ReturnAction")]
        public string ReturnRequestActionId { get; set; }
        public IList<ReturnRequestActionModel> AvailableReturnActions { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }

        #region Nested classes

        public partial class OrderItemModel : BaseNopEntityModel
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }
        }

        public partial class ReturnRequestReasonModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }
        public partial class ReturnRequestActionModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }

        #endregion
    }

}