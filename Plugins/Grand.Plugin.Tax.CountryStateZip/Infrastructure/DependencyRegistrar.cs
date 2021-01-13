using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<ITaxRateService,TaxRateService>();
            builder.AddScoped<CountryStateZipTaxProvider>();
        }

        public int Order
        {
            get { return 20; }
        }
    }
}
