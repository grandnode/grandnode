using Autofac;
using Autofac.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Widgets.NivoSlider.Controllers;

namespace Grand.Plugin.Widgets.NivoSlider.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //we cache presentation models between requests
            builder.RegisterType<WidgetsNivoSliderController>();
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
