using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class ReturnReqestListModel : BaseGrandModel
    {
        public ReturnReqestListModel()
        {
            ReturnRequestStatus = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.ReturnRequests.List.SearchCustomerEmail")]
        public string SearchCustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.List.SearchReturnRequestStatus")]
        public int SearchReturnRequestStatusId { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.List.GoDirectlyToId")]
        public string GoDirectlyToId { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [GrandResourceDisplayName("Admin.ReturnRequests.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        public string StoreId { get; set; }

        public IList<SelectListItem> ReturnRequestStatus { get; set; }
    }
}