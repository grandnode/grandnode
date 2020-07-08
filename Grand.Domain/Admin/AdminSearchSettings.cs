using Grand.Domain.Configuration;

namespace Grand.Domain.AdminSearch
{
    public class AdminSearchSettings : ISettings
    {
        public bool SearchInProducts { get; set; }

        public bool SearchInCategories { get; set; }

        public bool SearchInManufacturers { get; set; }

        public bool SearchInTopics { get; set; }

        public bool SearchInNews { get; set; }

        public bool SearchInBlogs { get; set; }

        public bool SearchInCustomers { get; set; }

        public bool SearchInOrders { get; set; }

        public int MinSearchTermLength { get; set; }

        public int MaxSearchResultsCount { get; set; }

        public int ProductsDisplayOrder { get; set; }

        public int CategoriesDisplayOrder { get; set; }

        public int ManufacturersDisplayOrder { get; set; }

        public int TopicsDisplayOrder { get; set; }

        public int NewsDisplayOrder { get; set; }

        public int BlogsDisplayOrder { get; set; }

        public int CustomersDisplayOrder { get; set; }

        public int OrdersDisplayOrder { get; set; }

        public bool SearchInMenu { get; set; }

        public int MenuDisplayOrder { get; set; }
    }
}
