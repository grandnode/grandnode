using Grand.Core.Plugins;
using Grand.Services.Common;
using System.Threading.Tasks;

namespace Grand.Plugin.Misc.EuropaCheckVat
{
    public class EuropaCheckVatPlugin : BasePlugin, IMiscPlugin
    {
        public override async Task Install()
        {
            await base.Install();
        }

        public override async Task Uninstall()
        {
            await base.Uninstall();
        }
    }
}
