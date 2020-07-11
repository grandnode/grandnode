using Grand.Domain.Catalog;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeConditionModel : BaseGrandModel
    {
        public ProductAttributeConditionModel()
        {
            ProductAttributes = new List<ProductAttributeModel>();
        }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Condition.EnableCondition")]
        public bool EnableCondition { get; set; }

        [GrandResourceDisplayName("Admin.Catalog.Products.ProductAttributes.Attributes.Condition.Attributes")]
        public string SelectedProductAttributeId { get; set; }
        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public string ProductAttributeMappingId { get; set; }
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