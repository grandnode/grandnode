using Grand.Core.Domain.Catalog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Services.Catalog.Tests
{
    [TestClass()]
    public class ProductExtensionsTests {
        [TestMethod()]
        public void Can_parse_allowed_quantities() {
            //simple parsing witohut unallowed word-characters
            var product = new Product { AllowedQuantities = "1,3,42,1,dsad,123,d22,d,122223" };

            var result = product.ParseAllowedQuantities();
            Assert.AreEqual(6, result.Length);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(42, result[2]);
            Assert.AreEqual(1, result[3]);
            Assert.AreEqual(123, result[4]);
            Assert.AreEqual(122223, result[5]);
        }

        [TestMethod()]
        public void Can_calculate_total_quantity_when_we_do_not_use_multiple_warehouses() {
            //if UseMultipleWarehouses is set to false, it will ignore StockQuantity of attached Warhouses
            var product = new Product {
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                UseMultipleWarehouses = false,
                StockQuantity = 8765
            };

            product.ProductWarehouseInventory.Add(new ProductWarehouseInventory { WarehouseId = "101", StockQuantity = 7 });
            product.ProductWarehouseInventory.Add(new ProductWarehouseInventory { WarehouseId = "111", StockQuantity = 8 });
            product.ProductWarehouseInventory.Add(new ProductWarehouseInventory { WarehouseId = "121", StockQuantity = -2 });

            Assert.AreEqual(8765, product.GetTotalStockQuantity(true));
        }

        [TestMethod()]
        public void Can_calculate_total_quantity_when_we_do_use_multiple_warehouses_with_reserved() {
            var product = new Product {
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                UseMultipleWarehouses = true, //UseMultipleWarehouse is set to true, so it will show only values of Warehouses
                StockQuantity = 333334765 //and totally ignores this
            };

            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "1", StockQuantity = 4, ReservedQuantity = 444 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "2", StockQuantity = 4, ReservedQuantity = 444 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "3", StockQuantity = 4, ReservedQuantity = 444 });

            //argument is true, so it will consider borh StockQuantity and ReservedQuantity
            //equation:
            //available quantity = StockQuantity - ReservedQuantity
            //-1320 = 12 - 1332
            //looks like our Warehouse is lacking of stuff in number -1320
            Assert.AreEqual(-1320, product.GetTotalStockQuantity(true));
        }

        [TestMethod()]
        public void Can_calculate_total_quantity_when_we_do_use_multiple_warehouses_without_reserved() {
            //if UseMultipleWarehouses is set to true, it will show StockQuantity of attached Warhouses
            var product = new Product {
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                UseMultipleWarehouses = true, //so ignore this Product's StockQuantity
                StockQuantity = 8765
            };

            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "1", StockQuantity = 4, ReservedQuantity = 444 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "2", StockQuantity = 4, ReservedQuantity = 222 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "3", StockQuantity = 4, ReservedQuantity = 111 });

            //it will ignore ReservedQuantity.. it is important to sell stuff, not having stuff
            Assert.AreEqual(12, product.GetTotalStockQuantity(false));
        }

        [TestMethod()]
        public void Can_calculate_total_quantity_when_we_do_use_multiple_warehouses_with_warehouse_specified() {
            //show only specified Warehouse (by WarehouseID) 
            var product = new Product {
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                UseMultipleWarehouses = true,
                StockQuantity = 8765
            };

            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "321", StockQuantity = 0, ReservedQuantity = 0 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "654", StockQuantity = 1500, ReservedQuantity = 1501 });
            product.ProductWarehouseInventory.Add(
                new ProductWarehouseInventory { WarehouseId = "987", StockQuantity = 0, ReservedQuantity = 0 });

            //only warehouse with ID = 654,
            //-1 = 1500-1501
            Assert.AreEqual(-1, product.GetTotalStockQuantity(true, "654"));
        }

    }
}