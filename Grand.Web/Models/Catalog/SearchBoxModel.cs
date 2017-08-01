using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class SearchBoxModel : BaseGrandModel
    {
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }
    }
}