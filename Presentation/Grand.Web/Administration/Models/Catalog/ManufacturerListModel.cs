using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Admin.Models.Catalog
{
    public partial class ManufacturerListModel : BaseGrandModel
    {
        public ManufacturerListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
        [AllowHtml]
        public string SearchManufacturerName { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}