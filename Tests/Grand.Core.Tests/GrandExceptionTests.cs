using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Core.Tests
{
    [TestClass()]
    public class GrandExceptionTests {
        [TestMethod()]
        public void pass_individual_message_to_exception() {
            try {
                throw new GrandException("lorem ipsum 123");
            }
            catch(Exception ex) {
                Assert.AreEqual("lorem ipsum 123", ex.Message);
            }            
        }
    }
}