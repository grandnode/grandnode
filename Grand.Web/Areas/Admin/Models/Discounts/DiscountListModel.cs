using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Discounts
{
    public partial class DiscountListModel : BaseGrandModel
    {
        public DiscountListModel()
        {
            AvailableDiscountTypes = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountCouponCode")]
        
        public string SearchDiscountCouponCode { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountName")]
        
        public string SearchDiscountName { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountType")]
        public int SearchDiscountTypeId { get; set; }
        public IList<SelectListItem> AvailableDiscountTypes { get; set; }
    }
}