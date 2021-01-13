using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<ITaxRateService,TaxRateService>();
            serviceCollection.AddScoped<CountryStateZipTaxProvider>();
        }

        public int Order
        {
            get { return 20; }
        }
    }
}
