using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Templates;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Templates
{
    [Validator(typeof(CategoryTemplateValidator))]
    public partial class CategoryTemplateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Category.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.ViewPath")]
        
        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}