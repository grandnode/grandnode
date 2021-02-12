using Grand.Core.Configuration;
using Grand.Core.DependencyInjection;
using Grand.Core.Plugins;
using Grand.Core.TypeFinders;
using Grand.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class DependencyInjection : IDependencyInjection
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<FacebookPixelPlugin>();
            if (PluginManager.FindPlugin(GetType()).Installed)
            {
                serviceCollection.AddScoped<IConsentCookie, FacebookPixelConsentCookie>();
            }
        }

        public int Order {
            get { return 10; }
        }
    }

}
