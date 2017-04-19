using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Localization;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Localization
{
    public partial class LanguageResourceFilterModel : BaseNopEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceName")]
        public string ResourceName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceValue")]
        public string ResourceValue { get; set; }

    }
}