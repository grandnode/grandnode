using Nop.Core.Domain.Localization;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Localization
{
    [TestFixture]
    public class LocaleStringResourcePersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_lst()
        {
            var lst = new LocaleStringResource
            {
                ResourceName = "ResourceName1",
                ResourceValue = "ResourceValue2",
                LanguageId = 1
            };

            var fromDb = SaveAndLoadEntity(lst);
            fromDb.ShouldNotBeNull();
            fromDb.ResourceName.ShouldEqual("ResourceName1");
            fromDb.ResourceValue.ShouldEqual("ResourceValue2");

        }
    }
}
