using Grand.Core.Configuration;
using Grand.Core.DependencyInjection;
using Grand.Core.TypeFinders;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Payments.PayPalStandard
{
    public class DependencyInjection : IDependencyInjection
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<PayPalStandardPaymentProcessor>();
        }

        public int Order {
            get { return 10; }
        }
    }
}
