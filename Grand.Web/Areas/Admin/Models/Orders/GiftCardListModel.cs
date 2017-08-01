using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class GiftCardListModel : BaseGrandModel
    {
        public GiftCardListModel()
        {
            ActivatedList = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.GiftCards.List.CouponCode")]
        
        public string CouponCode { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.List.RecipientName")]
        
        public string RecipientName { get; set; }

        [GrandResourceDisplayName("Admin.GiftCards.List.Activated")]
        public int ActivatedId { get; set; }
        [GrandResourceDisplayName("Admin.GiftCards.List.Activated")]
        public IList<SelectListItem> ActivatedList { get; set; }
    }
}