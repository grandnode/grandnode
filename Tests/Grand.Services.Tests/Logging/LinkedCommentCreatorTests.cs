using System.Collections.Generic;
using Grand.Services.Logging;
using Grand.Services.Logging.ActivityLogComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Services.Tests.Logging
{
    
    [TestClass()]
    public class LinkedCommentCreatorTests
    {
        private ILinkedCommentCreator _linkedCommentFormatter;

        [TestInitialize()]
        public void TestInitialize()
        {
            IActivityKeywordsProvider activityKeywordsProvider = new ActivityKeywordsProvider();
            IActivityEntityKeywordsProvider activityEntityKeywords = new ActivityEntityKeywordsProvider(activityKeywordsProvider);
            _linkedCommentFormatter = new LinkedCommentCreator(activityEntityKeywords);
        }

        [TestMethod]
        public void IfNoParameters_PlainTextShouldBeReturned()
        {
            var expected = "Edited settings";

            string result = _linkedCommentFormatter.CreateLinkedComment("EditSettings", "", "Edited settings", new object[0]);
            Assert.AreEqual(expected, result);
        }

        public static IEnumerable<object[]> NotSupportedByFormatterActivityKeywords()
        {
            yield return new object[] {
                "SendEmailFromAdminPanel",
                "5ea45e9c8258183ea0dbcb4c",
                "Email (from admin panel) {0}",
                new object[] {"antigoniapower@gmail.com"},
                "Email (from admin panel) antigoniapower@gmail.com",

            };
            yield return new object[] {
                "DeleteProduct",
                "5ea45e9c8258183ea0dbcb4c",
                "Deleted a product ('{0}')",
                new object[] { "Elegant Gemstone Necklace" },
                "Deleted a product ('Elegant Gemstone Necklace')",
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(NotSupportedByFormatterActivityKeywords), DynamicDataSourceType.Method)]
        public void IfSystemKeywordNotSupported_PlainTextShouldBeReturned(string systemKeyword, string entityKeyId,
            string commentBase, object[] commentParams, string expected)
        {
            string result = _linkedCommentFormatter.CreateLinkedComment(systemKeyword, entityKeyId, commentBase, commentParams);

            Assert.AreEqual(expected, result);
        }

        public static IEnumerable<object[]> SupportedByFormatterActivityKeywords()
        {
            yield return new object[] {
                "AddNewSpecAttribute",
                "5eb8351c92301e734412a1b6",
                "Added a new specification attribute ('{0}')",
                new []{ "Nikola" },
                "Added a new specification attribute ('<a href=\"/Admin/SpecificationAttribute/Edit/5eb8351c92301e734412a1b6\">Nikola</a>')",
                
            };
            yield return new object[] {
                "EditProduct",
                "5ea45e9d8258183ea0dbcd1e",
                "Edited a product ('{0}')",
                new [] { "Apple iCam" },
                "Edited a product ('<a href=\"/Admin/Product/Edit/5ea45e9d8258183ea0dbcd1e\">Apple iCam</a>')",
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(SupportedByFormatterActivityKeywords), DynamicDataSourceType.Method)]
        public void IfSystemKeywordSupported_TextWithLinkShouldBeReturned(string systemKeyword, string entityKeyId,
            string commentBase, object[] commentParams, string expected)
        {
            string result = _linkedCommentFormatter.CreateLinkedComment(systemKeyword, entityKeyId, commentBase, commentParams);

            Assert.AreEqual(expected, result);
        }
    }
}