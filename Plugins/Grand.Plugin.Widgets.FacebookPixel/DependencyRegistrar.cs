﻿using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;

namespace Grand.Plugin.Widgets.FacebookPixel
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<FacebookPixelPlugin>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
