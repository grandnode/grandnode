using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Grand.Admin.Models.Messages
{
    public partial class NewsLetterSubscriptionListModel : BaseNopModel
    {
        public NewsLetterSubscriptionListModel()
        {
            AvailableStores = new List<SelectListItem>();
            ActiveList = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
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

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.CustomerRoles")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.Categories")]
        [AllowHtml]
        public IList<SelectListItem> AvailableCategories { get; set; }

        [GrandResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.List.Category")]
        [UIHint("MultiSelect")]
        public IList<string> SearchCategoryIds { get; set; }

    }
}