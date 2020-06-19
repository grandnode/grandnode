using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Core.Domain.Logging;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Logging.ActivityLogComment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Services.Tests.Logging
{
    [TestClass()]
    public class LinkedCommentFormatterTests
    {
        private ILinkedCommentFormatter _linkedCommentFormatter;

        [TestInitialize()]
        public void TestInitialize()
        {
            ICustomerActivityService customerActivityService = new CustomerActivityServiceMock().Object;
            ILocalizationService localizationService = new LocalizationServiceMock().Object;
            IActivityKeywordsProvider activityKeywordsProvider = new ActivityKeywordsProvider();
            IActivityEntityKeywordsProvider activityEntityKeywords = new ActivityEntityKeywordsProvider(activityKeywordsProvider);
            _linkedCommentFormatter = new LinkedCommentFormatter(customerActivityService, localizationService, activityEntityKeywords);
        }

        [TestMethod]
        public async Task AlreadyFormattedComment_ShouldNotBeChanged()
        {
            var activityLog = new ActivityLog {
                ActivityLogTypeId = "5ea45e9c8258183ea0dbcb25",
                EntityKeyId = "5ea45e888258183ea0dbaf23",
                Comment = "Edited a topic ('<a href=\"/Admin/Topic/Edit/5ea45e888258183ea0dbaf23\">ApplyVendor</a>')"
            };
            var expected = "Edited a topic ('<a href=\"/Admin/Topic/Edit/5ea45e888258183ea0dbaf23\">ApplyVendor</a>')";

            string result = await _linkedCommentFormatter.AddLinkToPlainComment(activityLog);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public async Task CommentWithoutParameter_ShouldNotBeChanged()
        {
            var activityLog = new ActivityLog {
                ActivityLogTypeId = "5ea45e9c8258183ea0dbcb23",
                EntityKeyId = "",
                Comment = "Edited settings"
            };
            var expected = "Edited settings";

            string result = await _linkedCommentFormatter.AddLinkToPlainComment(activityLog);
            Assert.AreEqual(expected, result);
        }

        public static IEnumerable<object[]> NotSupportedByFormatterActivityKeywordLogs()
        {
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb4c",
                    EntityKeyId = "",
                    Comment = "Email (from admin panel) antigoniapower@gmail.com"
                },
                "Email (from admin panel) antigoniapower@gmail.com"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb0d",
                    EntityKeyId = "5ea45e9e8258183ea0dbcdfa",
                    Comment = "Deleted a product ('Elegant Gemstone Necklace')"
                },
                "Deleted a product ('Elegant Gemstone Necklace')"
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(NotSupportedByFormatterActivityKeywordLogs), DynamicDataSourceType.Method)]
        public async Task NotSupportedByFormatterActivityKeywordLogs_ShouldNotBeChanged(ActivityLog activityLog, string expected)
        {
            string result = await _linkedCommentFormatter.AddLinkToPlainComment(activityLog);

            Assert.AreEqual(expected, result);
        }

        public static IEnumerable<object[]> NotFormattedActivityLogs()
        {
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb1e",
                    EntityKeyId = "5eb80fbcca703c733490b2c7",
                    Comment = "Edited an order (ID = 5eb80fbcca703c733490b2c7). See order notes for details"
                },
                "Edited an order (ID = <a href=\"/Admin/Order/Edit/5eb80fbcca703c733490b2c7\">5eb80fbcca703c733490b2c7</a>). See order notes for details"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcafe",
                    EntityKeyId = "5eb8351c92301e734412a1b6",
                    Comment = "Added a new specification attribute ('Nikola')"
                },
                "Added a new specification attribute ('<a href=\"/Admin/SpecificationAttribute/Edit/5eb8351c92301e734412a1b6\">Nikola</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb24",
                    EntityKeyId = "5eb8351c92301e734412a1b6",
                    Comment = "Edited a specification attribute ('Nikola')"
                },
                "Edited a specification attribute ('<a href=\"/Admin/SpecificationAttribute/Edit/5eb8351c92301e734412a1b6\">Nikola</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb1f",
                    EntityKeyId = "5ea45e9d8258183ea0dbcd1e",
                    Comment = "Edited a product ('Apple iCam')"
                },
                "Edited a product ('<a href=\"/Admin/Product/Edit/5ea45e9d8258183ea0dbcd1e\">Apple iCam</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb0d",
                    EntityKeyId = "5ea45e9e8258183ea0dbcdfa",
                    Comment = "Deleted a product ('Elegant Gemstone Necklace')"
                },
                "Deleted a product ('Elegant Gemstone Necklace')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb14",
                    EntityKeyId = "5ea45e9c8258183ea0dbcb90",
                    Comment = "Edited a category ('Computers')"
                },
                "Edited a category ('<a href=\"/Admin/Category/Edit/5ea45e9c8258183ea0dbcb90\">Computers</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb1d",
                    EntityKeyId = "5ea45e9d8258183ea0dbcbc5",
                    Comment = "Edited a manufacturer ('Nike')"
                },
                "Edited a manufacturer ('<a href=\"/Admin/Manufacturer/Edit/5ea45e9d8258183ea0dbcbc5\">Nike</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb4f",
                    EntityKeyId = "5ec6a29f57c49f52cc14450e",
                    Comment = "Created knowledgebase category ('Test Category')"
                },
                "Created knowledgebase category ('<a href=\"/Admin/Knowledgebase/EditCategory/5ec6a29f57c49f52cc14450e\">Test Category</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb51",
                    EntityKeyId = "5ec6a2cf57c49f52cc14456a",
                    Comment = "Created knowledgebase article ('sdfsdf')"
                },
                "Created knowledgebase article ('<a href=\"/Admin/Knowledgebase/EditArticle/5ec6a2cf57c49f52cc14456a\">sdfsdf</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcaf3",
                    EntityKeyId = "5ec7f9ccdb062b73a4834c3d",
                    Comment = "Added a new contact attribute ('ContactAttributeTest')"
                },
                "Added a new contact attribute ('<a href=\"/Admin/ContactAttribute/Edit/5ec7f9ccdb062b73a4834c3d\">ContactAttributeTest</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcaf5",
                    EntityKeyId = "5ec7fbf3db062b73a483511a",
                    Comment = "Added a new customer role ('TestRole')"
                },
                "Added a new customer role ('<a href=\"/Admin/CustomerRole/Edit/5ec7fbf3db062b73a483511a\">TestRole</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcaf6",
                    EntityKeyId = "5ec7fc7cdb062b73a4835160",
                    Comment = "Added a new discount ('TestDiscount')"
                },
                "Added a new discount ('<a href=\"/Admin/Discount/Edit/5ec7fc7cdb062b73a4835160\">TestDiscount</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcaf9",
                    EntityKeyId = "5ec7fcc2db062b73a483549d",
                    Comment = "Added a new gift card ('a1ebf57e-f18b')"
                },
                "Added a new gift card ('<a href=\"/Admin/GiftCard/Edit/5ec7fcc2db062b73a483549d\">a1ebf57e-f18b</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb28",
                    EntityKeyId = "5ec7fd03db062b73a48355db",
                    Comment = "Add Interactive form - TestInteractiveForm"
                },
                "Add Interactive form - <a href=\"/Admin/InteractiveForm/Edit/5ec7fd03db062b73a48355db\">TestInteractiveForm</a>"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb20",
                    EntityKeyId = "5ea45e9c8258183ea0dbcb8d",
                    Comment = "Edited a product attribute ('Size')"
                },
                "Edited a product attribute ('<a href=\"/Admin/ProductAttribute/Edit/5ea45e9c8258183ea0dbcb8d\">Size</a>')"
            };
            yield return new object[] {
                new ActivityLog {
                    ActivityLogTypeId = "5ea45e9c8258183ea0dbcb25",
                    EntityKeyId = "5ea45e888258183ea0dbaf23",
                    Comment = "Edited a topic ('ApplyVendor')"
                },
                "Edited a topic ('<a href=\"/Admin/Topic/Edit/5ea45e888258183ea0dbaf23\">ApplyVendor</a>')"
            };
        }
        
        [DataTestMethod]
        [DynamicData(nameof(NotFormattedActivityLogs), DynamicDataSourceType.Method)]
        public async Task NotFormattedComment_ShouldBeFormattedWitLink(ActivityLog activityLog, string expected)
        {
            string result = await _linkedCommentFormatter.AddLinkToPlainComment(activityLog);

            Assert.AreEqual(expected, result);
        }
    }
}