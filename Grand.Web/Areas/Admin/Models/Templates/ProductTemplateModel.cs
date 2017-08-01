using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Templates;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Templates
{
    [Validator(typeof(ProductTemplateValidator))]
    public partial class ProductTemplateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Product.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Product.ViewPath")]
        
        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Product.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}