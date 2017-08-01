using Autofac;
using Autofac.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Data;
using Grand.Plugin.Payments.PayInStore.Controllers;

namespace Grand.Plugin.Payments.PayInStore
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            //base payment controller
            builder.RegisterType<PaymentPayInStoreController>();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
