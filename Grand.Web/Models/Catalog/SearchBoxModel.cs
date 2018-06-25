using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class SearchBoxModel : BaseGrandModel
    {
        public SearchBoxModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }
        public string SearchCategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
    }
}