using Autofac;
using Autofac.Core;
using Grand.Core.Caching;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Widgets.Slider.Controllers;

namespace Grand.Plugin.Widgets.Slider.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            //we cache presentation models between requests
            builder.RegisterType<WidgetsSliderController>();
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
