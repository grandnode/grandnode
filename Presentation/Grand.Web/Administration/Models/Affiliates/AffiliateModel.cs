using System;
using System.Web.Mvc;
using Grand.Admin.Models.Common;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Affiliates
{
    public partial class AffiliateModel : BaseNopEntityModel
    {
        public AffiliateModel()
        {
            Address = new AddressModel();
        }

        [NopResourceDisplayName("Admin.Affiliates.Fields.ID")]
        public override string Id { get; set; }

        [NopResourceDisplayName("Admin.Affiliates.Fields.URL")]
        public string Url { get; set; }


        [NopResourceDisplayName("Admin.Affiliates.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Affiliates.Fields.FriendlyUrlName")]
        [AllowHtml]
        public string FriendlyUrlName { get; set; }
        
        [NopResourceDisplayName("Admin.Affiliates.Fields.Active")]
        public bool Active { get; set; }

        public AddressModel Address { get; set; }

        #region Nested classes
        
        public partial class AffiliatedOrderModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Affiliates.Orders.Order")]
            public override string Id { get; set; }

            [NopResourceDisplayName("Admin.Affiliates.Orders.OrderStatus")]
            public string OrderStatus { get; set; }

            [NopResourceDisplayName("Admin.Affiliates.Orders.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [NopResourceDisplayName("Admin.Affiliates.Orders.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [NopResourceDisplayName("Admin.Affiliates.Orders.OrderTotal")]
            public string OrderTotal { get; set; }

            [NopResourceDisplayName("Admin.Affiliates.Orders.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public partial class AffiliatedCustomerModel : BaseNopEntityModel
        {
            [NopResourceDisplayName("Admin.Affiliates.Customers.Name")]
            public string Name { get; set; }
        }

        #endregion
    }
}