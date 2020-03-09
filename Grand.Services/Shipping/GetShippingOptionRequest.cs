using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using System.Collections.Generic;

namespace Grand.Services.Shipping
{
    /// <summary>
    /// Represents a request for getting shipping rate options
    /// </summary>
    public partial class GetShippingOptionRequest
    {
        #region Ctor

        public GetShippingOptionRequest()
        {
            Items = new List<PackageItem>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets a shopping cart items
        /// </summary>
        public IList<PackageItem> Items { get; set; }

        /// <summary>
        /// Gets or sets a shipping address (where we ship to)
        /// </summary>
        public Address ShippingAddress { get; set; }

        /// <summary>
        /// Shipped from warehouse
        /// </summary>
        public Warehouse WarehouseFrom { get; set; }
        /// <summary>
        /// Shipped from country
        /// </summary>
        public Country CountryFrom { get; set; }
        /// <summary>
        /// Shipped from state/province
        /// </summary>
        public StateProvince StateProvinceFrom { get; set; }
        /// <summary>
        /// Shipped from zip/postal code
        /// </summary>
        public string ZipPostalCodeFrom { get; set; }
        /// <summary>
        /// Shipped from city
        /// </summary>
        public string CityFrom { get; set; }
        /// <summary>
        /// Shipped from address
        /// </summary>
        public string AddressFrom { get; set; }

        /// <summary>
        /// Limit to store (identifier)
        /// </summary>
        public string StoreId { get; set; }

        #endregion

        #region Nested classes

        public class PackageItem
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="sci">Shopping cart item</param>
            /// <param name="qty">Override "Quantity" property of shopping cart item</param>
            public PackageItem(ShoppingCartItem sci, int? qty = null)
            {
                this.ShoppingCartItem = sci;
                this.OverriddenQuantity = qty;
            }

            /// <summary>
            /// Shopping cart item
            /// </summary>
            public ShoppingCartItem ShoppingCartItem { get; set; }
            /// <summary>
            /// If specified, override "Quantity" property of "ShoppingCartItem
            /// </summary>
            public int? OverriddenQuantity { get; set; }

            public int GetQuantity()
            {
                if (OverriddenQuantity.HasValue)
                    return OverriddenQuantity.Value;

                return ShoppingCartItem.Quantity;
            }
        }

        #endregion
    }
}
