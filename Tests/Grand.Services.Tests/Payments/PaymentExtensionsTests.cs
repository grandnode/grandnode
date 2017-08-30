using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Payments.Tests
{
    [TestClass()]
    public class PaymentExtensionsTests
    {
        [TestMethod()]
        public void Can_deserialize_empty_string()
        {
            //passsing "", shouldn't be null
            ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();
            Dictionary<string, object> deserialized = processPaymentRequest.DeserializeCustomValues("");

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Count());
        }

        [TestMethod()]
        public void Can_deserialize_null_string()
        {
            //passsing null, shouldn't be null
            ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();
            Dictionary<string, object> deserialized = processPaymentRequest.DeserializeCustomValues(null);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Count());
        }

        [TestMethod()]
        public void Can_serialize_and_deserialize_empty_CustomValues()
        {
            //passsing serialized empty object
            ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();
            string serializedXML = processPaymentRequest.SerializeCustomValues();
            Dictionary<string, object> deserialized = processPaymentRequest.DeserializeCustomValues(serializedXML);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Count());
        }

        [TestMethod()]
        public void Can_serialize_and_deserialize_CustomValues()
        {
            //checks if conversations are valid
            //null into ""
            //integral into string
            //no changes within special characters like < >
            ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();
            processPaymentRequest.CustomValues.Add("key01", "value01");
            processPaymentRequest.CustomValues.Add("key02", null);
            processPaymentRequest.CustomValues.Add("key03", 331233);
            processPaymentRequest.CustomValues.Add("<key 04>", "<value 01>");

            string serializedXML = processPaymentRequest.SerializeCustomValues();
            Dictionary<string, object> deserialized = processPaymentRequest.DeserializeCustomValues(serializedXML);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(4, deserialized.Count);

            Assert.IsTrue(deserialized.ContainsKey("key01"));
            Assert.AreEqual("value01", deserialized["key01"]);

            Assert.IsTrue(deserialized.ContainsKey("key02"));
            Assert.AreEqual("", deserialized["key02"]);

            Assert.IsTrue(deserialized.ContainsKey("key03"));
            Assert.AreEqual("331233", deserialized["key03"]);

            Assert.IsTrue(deserialized.ContainsKey("<key 04>"));
            Assert.AreEqual("<value 01>", deserialized["<key 04>"]);
        }
    }
}