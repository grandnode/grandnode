using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class NewsLetterSubscriptionListModel : BaseGrandModel
    {
        public NewsLetterSubscriptionListModel()
        {
            AvailableStores = new List<SelectListItem>();
            ActiveList = new List<SelectListItem>();
            SearchCategoryIds = new List<string>();
            AvailableCategories = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.SearchStore")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.SearchActive")]
        public int ActiveId { get; set; }
        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.SearchActive")]
        public IList<SelectListItem> ActiveList { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.Categories")]
        
        public IList<SelectListItem> AvailableCategories { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.Category")]
        [UIHint("MultiSelect")]
        public IList<string> SearchCategoryIds { get; set; }

    }
}