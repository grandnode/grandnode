using System;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Affiliates
{
    public partial class AffiliateModel : BaseGrandEntityModel
    {
        public AffiliateModel()
        {
            Address = new AddressModel();
        }

        [GrandResourceDisplayName("Admin.Affiliates.Fields.ID")]
        public override string Id { get; set; }

        [GrandResourceDisplayName("Admin.Affiliates.Fields.URL")]
        public string Url { get; set; }


        [GrandResourceDisplayName("Admin.Affiliates.Fields.AdminComment")]
        
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Affiliates.Fields.FriendlyUrlName")]
        
        public string FriendlyUrlName { get; set; }
        
        [GrandResourceDisplayName("Admin.Affiliates.Fields.Active")]
        public bool Active { get; set; }

        public AddressModel Address { get; set; }

        #region Nested classes
        
        public partial class AffiliatedOrderModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Affiliates.Orders.Order")]
            public override string Id { get; set; }
            public int OrderNumber { get; set; }

            [GrandResourceDisplayName("Admin.Affiliates.Orders.OrderStatus")]
            public string OrderStatus { get; set; }

            [GrandResourceDisplayName("Admin.Affiliates.Orders.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [GrandResourceDisplayName("Admin.Affiliates.Orders.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [GrandResourceDisplayName("Admin.Affiliates.Orders.OrderTotal")]
            public string OrderTotal { get; set; }

            [GrandResourceDisplayName("Admin.Affiliates.Orders.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class AffiliatedCustomerModel : BaseGrandEntityModel
        {
            [GrandResourceDisplayName("Admin.Affiliates.Customers.Name")]
            public string Name { get; set; }
        }

        #endregion
    }
}