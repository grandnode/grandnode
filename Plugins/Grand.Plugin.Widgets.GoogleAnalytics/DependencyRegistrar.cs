using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.DependencyInjection;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Widgets.GoogleAnalytics
{
    public class DependencyInjection : IDependencyInjection
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<GoogleAnalyticPlugin>();
            if (PluginManager.FindPlugin(GetType()).Installed)
            {
                serviceCollection.AddScoped<IConsentCookie,GoogleAnalyticsConsentCookie>();
            }
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
