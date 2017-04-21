using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Settings
{
    public partial class SettingFilterModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Settings.Filter.Name")]
        public string SettingFilterName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Filter.Value")]
        public string SettingFilterValue { get; set; }

    }
}