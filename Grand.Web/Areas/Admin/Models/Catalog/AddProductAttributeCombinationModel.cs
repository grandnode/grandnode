using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Grand.Core.Domain.Catalog;
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;

using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class AddProductAttributeCombinationModel : BaseGrandModel
    {
        public AddProductAttributeCombinationModel()
        {
            ProductAttributes = new List<ProductAttributeModel>();
            Warnings = new List<string>();
        }
        
        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders")]
        public bool AllowOutOfStockOrders { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Sku")]
        public string Sku { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.ManufacturerPartNumber")]
        public string ManufacturerPartNumber { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Gtin")]
        public string Gtin { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.OverriddenPrice")]
        [UIHint("DecimalNullable")]
        public decimal? OverriddenPrice { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.NotifyAdminForQuantityBelow")]
        public int NotifyAdminForQuantityBelow { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public IList<string> Warnings { get; set; }

        public string ProductId { get; set; }

        #region Nested classes

        public partial class ProductAttributeModel : BaseGrandEntityModel
        {
            public ProductAttributeModel()
            {
                Values = new List<ProductAttributeValueModel>();
            }

            public string ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public partial class ProductAttributeValueModel : BaseGrandEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }
        #endregion
    }
}