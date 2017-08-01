
using Grand.Framework;
using Grand.Framework.Mvc;
using System.Collections.Generic;

using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class CategoryListModel : BaseGrandModel
    {
        public CategoryListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        
        public string SearchCategoryName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Categories.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}