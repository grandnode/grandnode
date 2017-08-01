using System.Collections.Generic;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class VendorNavigationModel : BaseGrandModel
    {
        public VendorNavigationModel()
        {
            this.Vendors = new List<VendorBriefInfoModel>();
        }

        public IList<VendorBriefInfoModel> Vendors { get; set; }

        public int TotalVendors { get; set; }
    }

    public partial class VendorBriefInfoModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }
    }
}