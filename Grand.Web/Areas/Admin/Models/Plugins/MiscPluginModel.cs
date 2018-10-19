using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Plugins
{
    public partial class MiscPluginModel : BaseGrandModel
    {
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Misc.Fields.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}