using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Tests {
    [TestClass()]
    public class NopExceptionTests {
        [TestMethod()]
        public void pass_individual_message_to_exception() {
            try {
                throw new NopException("lorem ipsum 123");
            }
            catch(Exception ex) {
                Assert.AreEqual("lorem ipsum 123", ex.Message);
            }            
        }
    }
}