using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class GiftCardListModel : BaseGrandModel
    {
        public GiftCardListModel()
        {
            ActivatedList = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.GiftCards.List.CouponCode")]
        [AllowHtml]
        public string CouponCode { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.List.RecipientName")]
        [AllowHtml]
        public string RecipientName { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.List.Activated")]
        public int ActivatedId { get; set; }
        [GrandResourceDisplayName("Admin.GiftCards.List.Activated")]
        public IList<SelectListItem> ActivatedList { get; set; }
    }
}