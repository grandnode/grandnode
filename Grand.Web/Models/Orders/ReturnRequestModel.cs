using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Orders
{
    public partial class ReturnRequestModel : BaseGrandModel
    {
        public ReturnRequestModel()
        {
            Items = new List<OrderItemModel>();
            AvailableReturnReasons = new List<ReturnRequestReasonModel>();
            AvailableReturnActions = new List<ReturnRequestActionModel>();
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
        }

        public string OrderId { get; set; }
        public int OrderNumber { get; set; }
        public string OrderCode { get; set; }
        public IList<OrderItemModel> Items { get; set; }
        
        public IList<ReturnRequestReasonModel> AvailableReturnReasons { get; set; }

        public IList<ReturnRequestActionModel> AvailableReturnActions { get; set; }

        [GrandResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public DateTime? PickupDate { get; set; }

        public string Result { get; set; }

        public string Error { get; set; }

        public IList<AddressModel> ExistingAddresses { get; set; }

        public bool NewAddressPreselected { get; set; }

        public AddressModel NewAddress { get; set; }

        public bool ShowPickupAddress { get; set; }

        public bool ShowPickupDate { get; set; }

        public bool PickupDateRequired { get; set; }

        #region Nested classes

        public partial class OrderItemModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string VendorId { get; set; }
            public string VendorName { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }

            [GrandResourceDisplayName("ReturnRequests.ReturnReason")]
            public string ReturnRequestReasonId { get; set; }

            [GrandResourceDisplayName("ReturnRequests.ReturnAction")]
            public string ReturnRequestActionId { get; set; }
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