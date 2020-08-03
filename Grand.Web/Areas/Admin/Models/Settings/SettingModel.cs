using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class SettingModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Value")]

        public string Value { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.StoreName")]
        public string Store { get; set; }
        public string StoreId { get; set; }
    }
}