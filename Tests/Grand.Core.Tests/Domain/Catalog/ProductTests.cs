using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Grand.Domain.Catalog.Tests
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
        public void Two_references_with_the_different_id_should_not_be_equal() {
            Product product01 = new Product { Id = "123" };
            Product product02 = new Product { Id = "321" };

            Assert.AreNotEqual(product01, product02, "Entities with different ids should not be equal");
        }

        [TestMethod()]
        public void Entities_with_same_id_but_different_type_should_not_be_equal() {
            string tempID = "123";
            Product product01 = new Product { Id = tempID };
            Category product02 = new Category { Id = tempID };

            Assert.AreNotEqual(product01, product02, "Entities of different types should not be equal, even if they have the same id");
        }

        
    }
}