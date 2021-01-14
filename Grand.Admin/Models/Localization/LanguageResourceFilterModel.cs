using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Localization
{
    public partial class LanguageResourceFilterModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceName")]
        public string ResourceName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Languages.ResourcesFilter.Fields.ResourceValue")]
        public string ResourceValue { get; set; }

    }
}