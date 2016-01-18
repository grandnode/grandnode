using Nop.Core.Domain.Localization;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Localization
{
    [TestFixture]
    public class LocalizedPropertyPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_localizedProperty()
        {
            var localizedProperty = new LocalizedProperty
            {
                LocaleKey = "LocaleKey 1",
                LocaleValue = "LocaleValue 1",
                LanguageId = 1,
            };

            var fromDb = SaveAndLoadEntity(localizedProperty);
            fromDb.ShouldNotBeNull();
            fromDb.LocaleKey.ShouldEqual("LocaleKey 1");
            fromDb.LocaleValue.ShouldEqual("LocaleValue 1");
            
        }
    }
}
