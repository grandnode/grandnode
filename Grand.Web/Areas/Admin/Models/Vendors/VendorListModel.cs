using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Vendors
{
    public partial class VendorListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Vendors.List.SearchName")]
        
        public string SearchName { get; set; }
    }
}