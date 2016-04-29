using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nop.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Nop.Core.Fakes;
using System.Collections.Specialized;

namespace Nop.Core.Tests {
    [TestClass()]
    public class WebHelperTests {
        private FakeHttpContext _fakeHttpContext;
        private IWebHelper _webHelper;

        [TestMethod()]
        public void Can_get_serverVariables() {

            NameValueCollection serverVariablesCollection = new NameValueCollection();
            serverVariablesCollection.Add("primeKey", "primeValue412233231");
            serverVariablesCollection.Add("secundeKey", "secundeValue984238978");

            //see 7th given argument
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            //I will now search by key ("name" paramterer in method)
            Assert.AreEqual("primeValue412233231", _webHelper.ServerVariables("primeKey"));
            Assert.AreEqual("secundeValue984238978", _webHelper.ServerVariables("secundeKey"));
            Assert.AreEqual(string.Empty, _webHelper.ServerVariables("ultimateKey"));
        }

        [TestMethod()]
        public void Can_get_queryString() {
            NameValueCollection queryStringParameters = new NameValueCollection();
            queryStringParameters.Add("primeKey", "primeValue412233231");
            queryStringParameters.Add("secundeKey", "secundeValue984238978");

            //see 5th given argument
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, queryStringParameters, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            Assert.AreEqual("primeValue412233231", _webHelper.QueryString<string>("primeKey"));
            Assert.AreEqual("secundeValue984238978", _webHelper.QueryString<string>("secundeKey"));
            Assert.AreEqual(null, _webHelper.QueryString<string>("nopeKey"));
        }

        [TestMethod()]
        public void Can_get_storeHost_without_ssl() {
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "http://" + link + "/"; //adding "http://" (not https !)
            Assert.AreEqual(link, _webHelper.GetStoreHost(false));
            //GetStoreHost(false) returns http://www.xaaxaxxaxa.com/
        }

        [TestMethod()]
        public void Can_get_storeHost_with_ssl() {
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "https://" + link + "/"; //adding "https://" (not http !)
            Assert.AreEqual(link, _webHelper.GetStoreHost(true));
            //GetStoreHost(true) returns https://www.xaaxaxxaxa.com/
        }

        [TestMethod()]
        public void Can_get_storeLocation_without_ssl() {
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "http://" + link + "/";
            Assert.AreEqual(link, _webHelper.GetStoreLocation(false));
        }

        [TestMethod()]
        public void Can_get_storeLocation_with_ssl() {
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "https://" + link + "/";
            Assert.AreEqual(link, _webHelper.GetStoreLocation(true));
        }

        [TestMethod()]
        public void Can_get_storeLocation_in_virtual_directory() {
            //returns http://www.xaaxaxxaxa.com/someFakePath/
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/someFakePath", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "http://" + link + "/somefakepath/";
            Assert.AreEqual(link, _webHelper.GetStoreLocation(false));
        }

        [TestMethod()]
        public void Get_storeLocation_should_return_lowerCased_result() {
            NameValueCollection serverVariablesCollection = new NameValueCollection();

            string link = "www.xaaxaxxaxa.com";
            serverVariablesCollection.Add("HTTP_HOST", link);
            _fakeHttpContext = new FakeHttpContext("~/SOMEanotherFAKEpath", "GET", null, null, null, null, null, serverVariablesCollection);
            _webHelper = new WebHelper(_fakeHttpContext);

            link = "http://" + link + "/someanotherfakepath/";
            Assert.AreEqual(link, _webHelper.GetStoreLocation(false));
        }

        [TestMethod()]
        public void Can_remove_queryString() {
            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            //first parameter (removes "param1=value1&")
            Assert.AreEqual("http://www.example.com/?param2=value2",
                _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value2", "param1"));
            //second parameter (removes "&param2=value2")
            Assert.AreEqual("http://www.example.com/?param1=value1",
                _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value2", "param2"));
            //non existing parameter (no remove)
            Assert.AreEqual("http://www.example.com/?param1=value1&param2=value2",
                _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value2", "param3"));
        }

        [TestMethod()]
        public void Can_remove_queryString_should_return_lowerCased_result() { 
            //should don't care about case in given: "http://www.eXaMPlE.coM/?paRam1=VaLue1&paRam2=value2"

            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            //first parameter (removes "param1=value1&")
            Assert.AreEqual("http://www.example.com/?param2=value2",
                _webHelper.RemoveQueryString("http://www.eXaMPlE.coM/?paRam1=VaLue1&paRam2=value2", "param1"));
        }

        [TestMethod()]
        public void Can_remove_queryString_should_ignore_input_parameter_case() {
            //should don't care about character case in passed argument: "pArAm1"

            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            //first parameter (removes "param1=value1&")
            Assert.AreEqual("http://www.example.com/?param2=value2",
                _webHelper.RemoveQueryString("http://www.example.com/?param1=value1&param2=value2", "pArAm1"));
        }

        [TestMethod()]
        public void Can_modify_queryString() {
            /*
            summary
                changes "param1=value1" into "param1=value3"
                or if particlar param doesn't exist - adds it at end
            */

            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            Assert.AreEqual("http://www.example.com/?param1=value3&param2=value2",
                _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value2", "param1=value3", null));
            Assert.AreEqual("http://www.example.com/?param1=value1&param2=value99",
                _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value2", "param2=value99", null));
            Assert.AreEqual("http://www.example.com/?param1=value1&param2=value2&param321=value1000",
                _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value2", "param321=value1000", null));
        }

        [TestMethod()]
        public void Can_modify_queryString_with_anchor() {
            //adds "#anchorrrtest" at end

            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            Assert.AreEqual("http://www.example.com/?param1=value3&param2=value2#anchorrrtest",
                _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value2", "param1=value3", "anchorrrtest"));
        }

        [TestMethod()]
        public void Can_modify_queryString_new_anchor_should_remove_previous_one() {
            //removes existsing "#existinganchor" and replaces with "#anotheranchor" at end

            _fakeHttpContext = new FakeHttpContext("~/", "GET", null, null, null, null, null, null);
            _webHelper = new WebHelper(_fakeHttpContext);

            Assert.AreEqual("http://www.example.com/?param1=value3&param2=value2#anotheranchor",
                _webHelper.ModifyQueryString("http://www.example.com/?param1=value1&param2=value2#existinganchor", "param1=value3", "anotheranchor"));
        }
    }
}