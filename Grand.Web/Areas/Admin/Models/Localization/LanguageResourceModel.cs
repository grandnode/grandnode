using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Localization;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Localization
{
    [Validator(typeof(LanguageResourceValidator))]
    public partial class LanguageResourceModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Value")]
        
        public string Value { get; set; }

        public string LanguageId { get; set; }
    }
}