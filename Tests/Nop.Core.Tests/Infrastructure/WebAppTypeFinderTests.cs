using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nop.Core.Infrastructure.Tests {
    [TestClass()]
    public class WebAppTypeFinderTests {
        interface IInterface { };
        class First01 : IInterface{ };
        class First02 : IInterface { };
        class First03 : IInterface { };
        class First04 : IInterface { };
        class First05 : IInterface { };
        class First06 : IInterface { };
        class First07 : IInterface { };
        class First08 { };

        [TestMethod()]
        public void Check_how_many_classes_inherit_from_this_IInterface() {
            AppDomainTypeFinder appDomainTypeFinder = new AppDomainTypeFinder();
            IEnumerable<Type> result = appDomainTypeFinder.FindClassesOfType<IInterface>();
            
            int sevenInheritingClasses = 7;
            Assert.AreEqual(sevenInheritingClasses, result.Count());
            Assert.IsTrue(result.Contains(typeof(First06)));
            Assert.IsFalse(result.Contains(typeof(First08)));
        }

        [TestMethod()]
        public void GetBinDirectoryTest() {
            string result = new WebAppTypeFinder().GetBinDirectory();
            Assert.IsTrue(result.Contains(@"grandnode\Tests\Nop.Core.Tests\bin\Debug"));
        }
    }
}