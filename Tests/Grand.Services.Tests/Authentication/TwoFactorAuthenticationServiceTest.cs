using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grand.Services.Authentication;
using Google.Authenticator;

namespace Grand.Services.Tests.Authentication
{
    [TestClass()]
    public class TwoFactorAuthenticationServiceTest
    {
        private TwoFactorAuthenticationService _twofa;
        private TwoFactorAuthenticator _twoFactorAuthenticator;
        private string _secretKey;
        private string _token;
        private QrCodeSetup _setupCode;
       
        [TestInitialize()]
        public void TestInitialize()
        {
            _twofa = new TwoFactorAuthenticationService();
            _twoFactorAuthenticator = new TwoFactorAuthenticator();
            _secretKey = "PJWUMZKAUUFQKJBAMD6VGJ6RULFVW4ZH";
            _token = _twoFactorAuthenticator.GetCurrentPIN(_secretKey);
            _setupCode = _twofa.GenerateQrCodeSetup(_secretKey);
        }

        [TestMethod()]
        public void Validate_TwoFactor_Token()
        {
            Assert.IsTrue(_twofa.AuthenticateTwoFactor(_secretKey, _token));
        }

        [TestMethod()]
        public void Generate_Setup_Code()
        {
            Assert.IsNotNull(_setupCode.ManualEntryQrCode);
            Assert.IsNotNull(_setupCode.QrCodeImageUrl);
        }
    }
}
