using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Localization;

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