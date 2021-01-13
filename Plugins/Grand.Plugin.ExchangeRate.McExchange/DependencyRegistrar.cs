using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.ExchangeRate.McExchange;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.ExchangeRate.EcbExchange
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<McExchangeRateProvider>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
