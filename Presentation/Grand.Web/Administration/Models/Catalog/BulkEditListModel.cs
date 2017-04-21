using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Catalog
{
    public partial class BulkEditListModel : BaseGrandModel
    {
        public BulkEditListModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchProductName")]
        [AllowHtml]
        public string SearchProductName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchCategory")]
        public string SearchCategoryId { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchManufacturer")]
        public string SearchManufacturerId { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }
        public IList<SelectListItem> AvailableProductTypes { get; set; }
        

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
    }
}