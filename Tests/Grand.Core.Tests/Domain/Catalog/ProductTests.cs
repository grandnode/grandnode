using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Core.Domain.Catalog.Tests
{
    [TestClass()]
    public class ProductTests {
        [TestMethod()]
        public void Two_transient_entities_should_not_be_equal() {
            Product product01 = new Product();
            Product product02 = new Product();
            
            Assert.AreNotEqual(product01, product02, "Different transient entities should not be equal");
            Assert.AreNotSame(product01, product02);
        }

        [TestMethod()]
        public void Two_references_with_the_same_id_should_be_equal() {
            string tempID = "123";
            Product product01 = new Product { Id = tempID };
            Product product02 = new Product { Id = tempID };
            
            Assert.AreEqual(product01, product02, "Encje z tym samym ID powinny byc równe");
            Assert.AreNotSame(product01, product02, "Encje z tym samym ID nie mogą być tym samym obiektem");
        }

        [TestMethod()]
        public void Two_references_with_the_different_id_should_not_be_equal() {
            Product product01 = new Product { Id = "123" };
            Product product02 = new Product { Id = "321" };

            Assert.AreNotEqual(product01, product02, "Encje z tym różnym ID nie mogą byc równe");
            Assert.AreNotSame(product01, product02, "Encje z tym samym ID nie mogą być tym samym obiektem");
        }

        [TestMethod()]
        public void Entities_with_same_id_but_different_type_should_not_be_equal() {
            string tempID = "123";
            Product product01 = new Product { Id = tempID };
            Category product02 = new Category { Id = tempID };

            Assert.AreNotEqual(product01, product02, "Encje z tym samym ID ale o innym typie (Product i Category) nie mogą byc równe");
        }

        [TestMethod()]
        public void Overloaded_operators_work_fine() {
            string tempID = "123";
            Product product01 = new Product { Id = tempID };
            Product product02 = new Product { Id = tempID };
            Product product03 = new Product { Id = "4444" };

            Assert.IsTrue(product01 == product02);
            Assert.IsTrue(product01 != product03);
        }
    }
}