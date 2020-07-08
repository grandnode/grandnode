using Grand.Domain.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Services.Security.Tests
{
    [TestClass()]
    public class EncryptionServiceTests
    {
        private IEncryptionService _encryptionService;
        private SecuritySettings _securitySettings;

        [TestInitialize()]
        public void TestInitialize()
        {
            _securitySettings = new SecuritySettings { EncryptionKey = "273ece6f97dd844d97dd8f4d" };
            _encryptionService = new EncryptionService(_securitySettings);
        }

        [TestMethod()]
        public void Can_hash()
        {
            string password = "MyLittleSecret"; //the same as in grandnode
            string saltKey = "salt1";
            string result = _encryptionService.CreatePasswordHash(password, saltKey);

            const string expected = "A07A9638CCE93E48E3F26B37EF7BDF979B8124D6";
            Assert.AreEqual(expected, result);
        }

        [TestMethod()]
        public void Can_encrypt_and_decrypt()
        {
            //encrypt and then decrypt password
            string password = "MyLittleSecret";
            string encrypted = _encryptionService.EncryptText(password);
            string decrypted = _encryptionService.DecryptText(encrypted);
            Assert.AreEqual(password, decrypted);
        }
    }
}