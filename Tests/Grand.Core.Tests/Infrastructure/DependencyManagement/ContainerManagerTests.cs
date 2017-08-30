using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Core.Infrastructure.DependencyManagement.Tests
{
    [TestClass()]
    public class ContainerManagerTests
    {

        public interface IInterface { }
        public class First : IInterface { }
        public class Second : IInterface { }
        public class Third : IInterface { }

        [TestMethod()]
        public void EnumerablesFromDifferentLifetimeScopesShouldReturnDifferentCollections()
        {
            ContainerBuilder rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<First>().As<IInterface>();
            IContainer rootCointainer = rootBuilder.Build();

            ILifetimeScope scopeA = rootCointainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Second>().As<IInterface>());
            IEnumerable<IInterface> arrayA = scopeA.Resolve<IEnumerable<IInterface>>().ToArray();

            ILifetimeScope scopeB = rootCointainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Third>().As<IInterface>());
            IEnumerable<IInterface> arrayB = scopeA.Resolve<IEnumerable<IInterface>>().ToArray();

            Assert.AreEqual(2, arrayA.Count());
            Assert.AreEqual(2, arrayB.Count());
        }
    }
}
