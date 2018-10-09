using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Affiliates
{
    public partial class AffiliateListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Affiliates.List.SearchFirstName")]
        
        public string SearchFirstName { get; set; }

        [GrandResourceDisplayName("Admin.Affiliates.List.SearchLastName")]
        
        public string SearchLastName { get; set; }

        [GrandResourceDisplayName("Admin.Affiliates.List.SearchFriendlyUrlName")]
        
        public string SearchFriendlyUrlName { get; set; }

        [GrandResourceDisplayName("Admin.Affiliates.List.LoadOnlyWithOrders")]
        public bool LoadOnlyWithOrders { get; set; }
        [GrandResourceDisplayName("Admin.Affiliates.List.OrdersCreatedFromUtc")]
        [UIHint("DateNullable")]
        public DateTime? OrdersCreatedFromUtc { get; set; }
        [GrandResourceDisplayName("Admin.Affiliates.List.OrdersCreatedToUtc")]
        [UIHint("DateNullable")]
        public DateTime? OrdersCreatedToUtc { get; set; }
    }
}