using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class ReturnRequestModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.ID")]
        public override string Id { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.ID")]
        public int ReturnNumber { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Order")]
        public string OrderId { get; set; }
        public int OrderNumber { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Customer")]
        public string CustomerInfo { get; set; }

        public string ProductId { get; set; }
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Product")]
        public string ProductName { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Quantity")]
        public int Quantity { get; set; }

        
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.ReasonForReturn")]
        public string ReasonForReturn { get; set; }

        
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.RequestedAction")]
        public string RequestedAction { get; set; }

        
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.CustomerComments")]
        public string CustomerComments { get; set; }

        
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.StaffNotes")]
        public string StaffNotes { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public int ReturnRequestStatusId { get; set; }
        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public string ReturnRequestStatusStr { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}