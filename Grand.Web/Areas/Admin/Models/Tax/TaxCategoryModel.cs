using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Tax;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Tax
{
    [Validator(typeof(TaxCategoryValidator))]
    public partial class TaxCategoryModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}