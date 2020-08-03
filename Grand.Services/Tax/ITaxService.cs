using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial interface ITaxService
    {
        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        ITaxProvider LoadActiveTaxProvider();

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        ITaxProvider LoadTaxProviderBySystemName(string systemName);

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        IList<ITaxProvider> LoadAllTaxProviders();


        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price, Customer customer);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price, bool includingTax, Customer customer);

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        /// <returns>Price</returns>
        Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, string taxCategoryId, decimal price, bool includingTax, Customer customer, bool priceIncludesTax);


        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="unitPrice">Unit Price</param>
        /// <param name="unitPricewithoutDisc">Unit price without discount</param>
        /// <param name="subTotal">Sub-Total</param>
        /// <param name="discountAmount">Discount amount</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        /// <returns>TaxProductPrice</returns>
        Task<TaxProductPrice> GetTaxProductPrice(
            Product product,
            Customer customer,
            decimal unitPrice,
            decimal unitPricewithoutDisc,
            decimal subTotal,
            decimal discountAmount,
            bool priceIncludesTax
            );

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal shippingPrice, decimal taxRate)> GetShippingPrice(decimal price, Customer customer);

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal shippingPrice, decimal taxRate)> GetShippingPrice(decimal price, bool includingTax, Customer customer);

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal paymentPrice, decimal taxRate)> GetPaymentMethodAdditionalFee(decimal price, Customer customer);

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal paymentPrice, decimal taxRate)> GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="ca">Checkout attribute</param>
        /// <param name="cav">Checkout attribute value</param>
        /// <returns>Price</returns>
        Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttribute ca, CheckoutAttributeValue cav);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="ca">Checkout attribute</param>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttribute ca, CheckoutAttributeValue cav, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="ca">Checkout attribute</param>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttribute ca, CheckoutAttributeValue cav, bool includingTax, Customer customer);

        /// <summary>
        /// Gets a value indicating whether a product is tax exempt
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>A value indicating whether a product is tax exempt</returns>
        bool IsTaxExempt(Product product, Customer customer);

        /// <summary>
        /// Gets a value indicating whether EU VAT exempt (the European Union Value Added Tax)
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        Task<bool> IsVatExempt(Address address, Customer customer);
    }
}
