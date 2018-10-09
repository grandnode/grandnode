using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public class AdminSearchSettingsModel
    {
        [GrandResourceDisplayName("AdminSearch.Fields.SearchInProducts")]
        public bool SearchInProducts { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.ProductsDisplayOrder")]
        public int ProductsDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInCategories")]
        public bool SearchInCategories { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.CategoriesDisplayOrder")]
        public int CategoriesDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInManufacturers")]
        public bool SearchInManufacturers { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.ManufacturersDisplayOrder")]
        public int ManufacturersDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInTopics")]
        public bool SearchInTopics { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.TopicsDisplayOrder")]
        public int TopicsDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInNews")]
        public bool SearchInNews { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.NewsDisplayOrder")]
        public int NewsDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInBlogs")]
        public bool SearchInBlogs { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.BlogsDisplayOrder")]
        public int BlogsDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInCustomers")]
        public bool SearchInCustomers { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.CustomersDisplayOrder")]
        public int CustomersDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInOrders")]
        public bool SearchInOrders { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.OrdersDisplayOrder")]
        public int OrdersDisplayOrder { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.MinSearchTermLength")]
        public int MinSearchTermLength { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.MaxSearchResultsCount")]
        public int MaxSearchResultsCount { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.SearchInMenu")]
        public bool SearchInMenu { get; set; }

        [GrandResourceDisplayName("AdminSearch.Fields.MenuDisplayOrder")]
        public int MenuDisplayOrder { get; set; }
    }
}
