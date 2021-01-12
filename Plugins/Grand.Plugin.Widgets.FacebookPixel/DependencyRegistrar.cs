using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<FacebookPixelPlugin>();
            if (PluginManager.FindPlugin(GetType()).Installed)
            {
                builder.AddScoped<IConsentCookie,FacebookPixelConsentCookie>();
            }
        }

        public int Order {
            get { return 10; }
        }
    }

}
