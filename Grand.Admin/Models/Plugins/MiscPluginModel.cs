using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Plugins
{
    public partial class MiscPluginModel : BaseModel
    {
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Misc.Fields.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}