using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Admin.Models.Catalog
{
    public partial class BulkEditListModel : BaseModel
    {
        public BulkEditListModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchProductName")]
        public string SearchProductName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchCategory")]
        public string SearchCategoryId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchManufacturer")]
        public string SearchManufacturerId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchStore")]
        public string SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableProductTypes { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}