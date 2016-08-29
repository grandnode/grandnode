using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Discounts
{
    public partial class DiscountListModel : BaseNopModel
    {
        public DiscountListModel()
        {
            AvailableDiscountTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountCouponCode")]
        [AllowHtml]
        public string SearchDiscountCouponCode { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountName")]
        [AllowHtml]
        public string SearchDiscountName { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Discounts.List.SearchDiscountType")]
        public int SearchDiscountTypeId { get; set; }
        public IList<SelectListItem> AvailableDiscountTypes { get; set; }
    }
}