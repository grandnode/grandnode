using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class LowStockProductModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        public string Name { get; set; }

        public string Attributes { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.ManageInventoryMethod")]
        public string ManageInventoryMethod { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
        public bool Published { get; set; }
    }
}