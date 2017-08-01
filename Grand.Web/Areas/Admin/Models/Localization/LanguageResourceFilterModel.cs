using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Localization;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Localization
{
    public partial class LanguageResourceFilterModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceName")]
        public string ResourceName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceValue")]
        public string ResourceValue { get; set; }

    }
}