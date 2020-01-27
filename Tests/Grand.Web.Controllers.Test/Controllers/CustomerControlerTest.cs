using System;
using System.Collections.Generic;
using System.Text;
using Grand.Web.Models.Customer;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Grand.Web.Test.Controllers
{
    [TestClass()]
    public class CustomerControlerTest
    {
        private TwoFactorAuthenticationModel _twoFactorAuthenticationModel;

        
        public bool TestInitialize()
        {
            //_twoFactorAuthenticationModel = new TwoFactorAuthenticationModel { UserUniqueKey };
        }

        [TestMethod()]
        public async Task GetTwoFactorAuthenticate_ReturnsAViewResult_WithA()
        {
                  
        }
    }
}
