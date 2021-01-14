
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Core.ModelBinding;

namespace Grand.Admin.Models.Catalog
{
    public partial class BulkEditProductModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.SKU")]
        
        public string Sku { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Price")]
        public decimal Price { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.ManageInventoryMethod")]
        public string ManageInventoryMethod { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Published")]
        public bool Published { get; set; }
    }
}