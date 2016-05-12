using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Models.Catalog
{
    public partial class ManufacturerListModel : BaseNopModel
    {
        public ManufacturerListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
        [AllowHtml]
        public string SearchManufacturerName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}