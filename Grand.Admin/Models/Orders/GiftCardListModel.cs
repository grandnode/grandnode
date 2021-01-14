using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Admin.Models.Orders
{
    public partial class GiftCardListModel : BaseModel
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