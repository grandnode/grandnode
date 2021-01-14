using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Settings
{
    public partial class SettingFilterModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Settings.Filter.Name")]
        public string SettingFilterName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Filter.Value")]
        public string SettingFilterValue { get; set; }

    }
}