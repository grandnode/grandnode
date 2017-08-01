using System.Collections.Generic;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Catalog
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