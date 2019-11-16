using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Core.Plugins;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grand.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial class TaxService : ITaxService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ICountryService _countryService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="addressService">Address service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="geoLookupService">GEO lookup service</param>
        /// <param name="countryService">Country service</param>
        /// <param name="logger">Logger service</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <param name="addressSettings">Address settings</param>
        public TaxService(IAddressService addressService,
            IWorkContext workContext,
            TaxSettings taxSettings,
            IPluginFinder pluginFinder,
            IGeoLookupService geoLookupService,
            ICountryService countryService,
            IServiceProvider serviceProvider,
            ILogger logger,
            CustomerSettings customerSettings,
            AddressSettings addressSettings)
        {
            _addressService = addressService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _pluginFinder = pluginFinder;
            _geoLookupService = geoLookupService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _countryService = countryService;
            _customerSettings = customerSettings;
            _addressSettings = addressSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get a value indicating whether a customer is consumer (a person, not a company) located in Europe Union
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        protected virtual async Task<bool> IsEuConsumer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            Country country = null;

            //get country from billing address
            if (_addressSettings.CountryEnabled && customer.BillingAddress != null)
            {
                var _country = await _countryService.GetCountryById(customer.BillingAddress.CountryId);
                country = _country;
            }
            //get country specified during registration?
            if (country == null && _customerSettings.CountryEnabled)
            {
                var countryId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                country = await _countryService.GetCountryById(countryId);
            }

            //get country by IP address
            if (country == null)
            {
                var ipAddress = customer.LastIpAddress;
                var countryIsoCode = _geoLookupService.LookupCountryIsoCode(ipAddress);
                country = await _countryService.GetCountryByTwoLetterIsoCode(countryIsoCode);
            }

            //we cannot detect country
            if (country == null)
                return false;

            //outside EU
            if (!country.SubjectToVat)
                return false;

            //company (business) or consumer?
            var customerVatStatus = (VatNumberStatus)customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId);
            if (customerVatStatus == VatNumberStatus.Valid)
                return false;

            //TODO: use specified company name? (both address and registration one)

            //consumer
            return true;
        }

        /// <summary>
        /// Create request for tax calculation
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Package for tax calculation</returns>
        protected virtual async Task<CalculateTaxRequest> CreateCalculateTaxRequest(Product product, 
            string taxCategoryId, Customer customer, decimal price)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var calculateTaxRequest = new CalculateTaxRequest();
            calculateTaxRequest.Customer = customer;
            calculateTaxRequest.Product = product;
            calculateTaxRequest.Price = price;

            if (!String.IsNullOrEmpty(taxCategoryId))
            {
                calculateTaxRequest.TaxCategoryId = taxCategoryId;
            }
            else
            {
                if (product != null)
                    calculateTaxRequest.TaxCategoryId = product.TaxCategoryId;
            }

            var basedOn = _taxSettings.TaxBasedOn;
            //new EU VAT rules starting January 1st 2015
            //find more info at http://ec.europa.eu/taxation_customs/taxation/vat/how_vat_works/telecom/index_en.htm#new_rules
            //EU VAT enabled?
            if (_taxSettings.EuVatEnabled)
            {
                //telecommunications, broadcasting and electronic services?
                if (product != null && product.IsTelecommunicationsOrBroadcastingOrElectronicServices)
                {
                    //January 1st 2015 passed?
                    if (DateTime.UtcNow > new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    {
                        //Europe Union consumer?
                        if (await IsEuConsumer(customer))
                        {
                            //We must charge VAT in the EU country where the customer belongs (not where the business is based)
                            basedOn = TaxBasedOn.BillingAddress;
                        }
                    }
                }
            }
            if (basedOn == TaxBasedOn.BillingAddress && customer.BillingAddress == null)
                basedOn = TaxBasedOn.DefaultAddress;
            if (basedOn == TaxBasedOn.ShippingAddress && customer.ShippingAddress == null)
                basedOn = TaxBasedOn.DefaultAddress;

            Address address;

            switch (basedOn)
            {
                case TaxBasedOn.BillingAddress:
                    {
                        address = customer.BillingAddress;
                    }
                    break;
                case TaxBasedOn.ShippingAddress:
                    {
                        address = customer.ShippingAddress;
                    }
                    break;
                case TaxBasedOn.DefaultAddress:
                default:
                    {
                        address = await _addressService.GetAddressByIdSettings(_taxSettings.DefaultTaxAddressId);
                    }
                    break;
            }

            calculateTaxRequest.Address = address;
            return calculateTaxRequest;
        }

        /// <summary>
        /// Calculated price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="percent">Percent</param>
        /// <param name="increase">Increase</param>
        /// <returns>New price</returns>
        protected virtual decimal CalculatePrice(decimal price, decimal percent, bool increase)
        {
            if (percent == decimal.Zero)
                return price;

            decimal result;
            if (increase)
            {
                result = price * (1 + percent / 100);
            }
            else
            {
                result = price - (price) / (100 + percent) * percent;
            }
            if (result == decimal.Zero)
                return 0;

            return Math.Round(result, 2);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <param name="price">Price (taxable value)</param>
        /// <param name="taxRate">Calculated tax rate</param>
        /// <param name="isTaxable">A value indicating whether a request is taxable</param>
        protected virtual async Task<(decimal taxRate, bool isTaxable)> GetTaxRate(Product product, string taxCategoryId, 
            Customer customer, decimal price)
        {
            decimal taxRate = decimal.Zero;
            bool isTaxable = true;

            //active tax provider
            var activeTaxProvider = LoadActiveTaxProvider();
            if (activeTaxProvider == null)
                return (taxRate, isTaxable);

            //tax request
            var calculateTaxRequest = await CreateCalculateTaxRequest(product, taxCategoryId, customer, price);

            //tax exempt
            if (IsTaxExempt(product, calculateTaxRequest.Customer))
            {
                isTaxable = false;
            }
            //make EU VAT exempt validation (the European Union Value Added Tax)
            if (isTaxable && 
                _taxSettings.EuVatEnabled &&
                await IsVatExempt(calculateTaxRequest.Address, calculateTaxRequest.Customer))
            {
                //VAT is not chargeable
                isTaxable = false;
            }

            //get tax rate
            var calculateTaxResult = await activeTaxProvider.GetTaxRate(calculateTaxRequest);
            if (calculateTaxResult.Success)
            {
                //ensure that tax is equal or greater than zero
                if (calculateTaxResult.TaxRate < decimal.Zero)
                    calculateTaxResult.TaxRate = decimal.Zero;
                
                taxRate = calculateTaxResult.TaxRate;
            }
            else
            {
                foreach (var error in calculateTaxResult.Errors)
                {
                    _logger.Error(string.Format("{0} - {1}", activeTaxProvider.PluginDescriptor.FriendlyName, error), null, customer);
                }
            }
            return (taxRate, isTaxable);
        }
        

        #endregion

        #region Methods

        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        public virtual ITaxProvider LoadActiveTaxProvider()
        {
            var taxProvider = LoadTaxProviderBySystemName(_taxSettings.ActiveTaxProviderSystemName);
            if (taxProvider == null)
                taxProvider = LoadAllTaxProviders().FirstOrDefault();
            return taxProvider;
        }

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        public virtual ITaxProvider LoadTaxProviderBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<ITaxProvider>(systemName);
            if (descriptor != null)
                return _serviceProvider.GetRequiredService(descriptor.PluginType) as ITaxProvider;

            return null;
        }

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        public virtual IList<ITaxProvider> LoadAllTaxProviders()
        {
            return _pluginFinder.GetPlugins<ITaxProvider>().ToList();
        }



        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price)
        {
            var customer = _workContext.CurrentCustomer;
            return await GetProductPrice(product, price, customer);
        }
        
        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price,
            Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return await GetProductPrice(product, price, includingTax, customer);
        }

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, decimal price,
            bool includingTax, Customer customer)
        {
            bool priceIncludesTax = _taxSettings.PricesIncludeTax;
            string taxCategoryId = "";
            return await GetProductPrice(product, taxCategoryId, price, includingTax, 
                customer, priceIncludesTax);
        }

        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal productprice, decimal taxRate)> GetProductPrice(Product product, string taxCategoryId,
            decimal price, bool includingTax, Customer customer,
            bool priceIncludesTax)
        {
            
            //no need to calculate tax rate if passed "price" is 0
            if (price == decimal.Zero)
            {
                return (0, 0);
            }

            var taxrates = await GetTaxRate(product, taxCategoryId, customer, price);

            if (priceIncludesTax)
            {
                //"price" already includes tax
                if (includingTax)
                {
                    //we should calculate price WITH tax
                    if (!taxrates.isTaxable)
                    {
                        //but our request is not taxable
                        //hence we should calculate price WITHOUT tax
                        price = CalculatePrice(price, taxrates.taxRate, false);
                    }
                }
                else
                {
                    //we should calculate price WITHOUT tax
                    price = CalculatePrice(price, taxrates.taxRate, false);
                }
            }
            else
            {
                //"price" doesn't include tax
                if (includingTax)
                {
                    //we should calculate price WITH tax
                    //do it only when price is taxable
                    if (taxrates.isTaxable)
                    {
                        price = CalculatePrice(price, taxrates.taxRate, true);
                    }
                }
            }

            if (!taxrates.isTaxable)
            {
                //we return 0% tax rate in case a request is not taxable
                taxrates.taxRate = decimal.Zero;
            }

            //allowed to support negative price adjustments
            //if (price < decimal.Zero)
            //    price = decimal.Zero;

            return (price, taxrates.taxRate);
        }

        public virtual async Task<TaxProductPrice> GetTaxProductPrice(
            Product product,
            Customer customer,
            decimal unitPrice,
            decimal unitPricewithoutDisc,
            decimal subTotal,
            decimal discountAmount,
            bool priceIncludesTax
            )
        {

            string taxCategoryId = ""; //it seems to be strange, but this way was used above
            var productPrice = new TaxProductPrice();

            //----------------------------------------------------------------------------------------------------
            var taxrates = await GetTaxRate(product, taxCategoryId, customer, 0);
            productPrice.taxRate = taxrates.taxRate;
            if (priceIncludesTax)
            {
                if (taxrates.isTaxable)
                {
                    productPrice.UnitPriceWihoutDiscInclTax = unitPricewithoutDisc;
                    productPrice.UnitPriceWihoutDiscExclTax = CalculatePrice(unitPricewithoutDisc, taxrates.taxRate, false);

                    productPrice.UnitPriceInclTax = unitPrice;
                    productPrice.UnitPriceExclTax = CalculatePrice(unitPrice, taxrates.taxRate, false);

                    productPrice.SubTotalInclTax = subTotal;
                    productPrice.SubTotalExclTax = CalculatePrice(subTotal, taxrates.taxRate, false);

                    productPrice.discountAmountInclTax = discountAmount;
                    productPrice.discountAmountExclTax = CalculatePrice(discountAmount, taxrates.taxRate, false);
                }
                else
                {
                    productPrice.UnitPriceWihoutDiscInclTax = CalculatePrice(unitPricewithoutDisc, taxrates.taxRate, false);
                    productPrice.UnitPriceWihoutDiscExclTax = CalculatePrice(unitPricewithoutDisc, taxrates.taxRate, false);

                    productPrice.UnitPriceInclTax = CalculatePrice(unitPrice, taxrates.taxRate, false);
                    productPrice.UnitPriceExclTax = CalculatePrice(unitPrice, taxrates.taxRate, false);

                    productPrice.SubTotalInclTax = CalculatePrice(subTotal, taxrates.taxRate, false);
                    productPrice.SubTotalExclTax = CalculatePrice(subTotal, taxrates.taxRate, false);

                    productPrice.discountAmountInclTax = CalculatePrice(discountAmount, taxrates.taxRate, false);
                    productPrice.discountAmountExclTax = CalculatePrice(discountAmount, taxrates.taxRate, false);
                }
            }
            else
            {
                if (taxrates.isTaxable)
                {
                    productPrice.UnitPriceWihoutDiscInclTax = CalculatePrice(unitPricewithoutDisc, taxrates.taxRate, true);
                    productPrice.UnitPriceWihoutDiscExclTax = unitPricewithoutDisc;

                    productPrice.UnitPriceInclTax = CalculatePrice(unitPrice, taxrates.taxRate, true);
                    productPrice.UnitPriceExclTax = unitPrice;

                    productPrice.SubTotalInclTax = CalculatePrice(subTotal, taxrates.taxRate, true);
                    productPrice.SubTotalExclTax = subTotal;

                    productPrice.discountAmountInclTax = CalculatePrice(discountAmount, taxrates.taxRate, true);
                    productPrice.discountAmountExclTax = discountAmount;
                }
                else
                {
                    productPrice.UnitPriceWihoutDiscInclTax = unitPricewithoutDisc;
                    productPrice.UnitPriceWihoutDiscExclTax = unitPricewithoutDisc;

                    productPrice.UnitPriceInclTax = unitPrice;
                    productPrice.UnitPriceExclTax = unitPrice;

                    productPrice.SubTotalInclTax = subTotal;
                    productPrice.SubTotalExclTax = subTotal;

                    productPrice.discountAmountInclTax = discountAmount;
                    productPrice.discountAmountExclTax = discountAmount;
                }
            }
           
            if (!taxrates.isTaxable)
            {
                //we return 0% tax rate in case a request is not taxable
                taxrates.taxRate = decimal.Zero;
            }
            return productPrice;
        }

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal shippingPrice, decimal taxRate)> GetShippingPrice(decimal price, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return await GetShippingPrice(price, includingTax, customer);
        }

        /// <summary>
        /// Gets shipping price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal shippingPrice, decimal taxRate)> GetShippingPrice(decimal price, bool includingTax, Customer customer)
        {
            if (!_taxSettings.ShippingIsTaxable)
            {
                return (price, 0);
            }

            string taxClassId = _taxSettings.ShippingTaxClassId;
            bool priceIncludesTax = _taxSettings.ShippingPriceIncludesTax;
            var prices = await GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax);
            return (prices.productprice, prices.taxRate);
        }

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal paymentPrice, decimal taxRate)> GetPaymentMethodAdditionalFee(decimal price, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return await GetPaymentMethodAdditionalFee(price, includingTax, customer);
        }

        /// <summary>
        /// Gets payment method additional handling fee
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal paymentPrice, decimal taxRate)> GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer)
        {
            if (!_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                return (price, 0);
            }
            string taxClassId = _taxSettings.PaymentMethodAdditionalFeeTaxClassId;
            bool priceIncludesTax = _taxSettings.PaymentMethodAdditionalFeeIncludesTax;
            var prices = await GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax);
            return (prices.productprice, prices.taxRate);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttributeValue cav)
        {
            var customer = _workContext.CurrentCustomer;
            return await GetCheckoutAttributePrice(cav, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttributeValue cav, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return await GetCheckoutAttributePrice(cav, includingTax, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual async Task<(decimal checkoutPrice, decimal taxRate)> GetCheckoutAttributePrice(CheckoutAttributeValue cav, bool includingTax, Customer customer)
        {
            if (cav == null)
                throw new ArgumentNullException("cav");

            var checkoutAttribute = await _serviceProvider.GetRequiredService<ICheckoutAttributeService>().GetCheckoutAttributeById(cav.CheckoutAttributeId);
            decimal price = cav.PriceAdjustment;
            if (checkoutAttribute.IsTaxExempt)
            {
                return (price, 0);
            }

            bool priceIncludesTax = _taxSettings.PricesIncludeTax;
            string taxClassId = checkoutAttribute.TaxCategoryId;
            var prices = await GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax);

            return (prices.productprice, prices.taxRate);
        }

        
        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual async Task<(VatNumberStatus status, string name, string address, Exception exception)> GetVatNumberStatus(string fullVatNumber)
        {
            string name = string.Empty;
            string address = string.Empty;

            if (String.IsNullOrWhiteSpace(fullVatNumber))
                return (VatNumberStatus.Empty, name, address, null);

            fullVatNumber = fullVatNumber.Trim();
            
            //GB 111 1111 111 or GB 1111111111
            //more advanced regex - http://codeigniter.com/wiki/European_Vat_Checker
            var r = new Regex(@"^(\w{2})(.*)");
            var match = r.Match(fullVatNumber);
            if (!match.Success)
                return (VatNumberStatus.Invalid, name, address, null);
            var twoLetterIsoCode = match.Groups[1].Value;
            var vatNumber = match.Groups[2].Value;

            return await GetVatNumberStatus(twoLetterIsoCode, vatNumber);
        }
       
        
        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual async Task<(VatNumberStatus status, string name, string address, Exception exception)> GetVatNumberStatus(string twoLetterIsoCode, string vatNumber)
        {
            string name = string.Empty;
            string address = string.Empty;

            if (String.IsNullOrEmpty(twoLetterIsoCode))
                return (VatNumberStatus.Empty, name, address, null);

            if (String.IsNullOrEmpty(vatNumber))
                return (VatNumberStatus.Empty, name, address, null);

            if (_taxSettings.EuVatAssumeValid)
                return (VatNumberStatus.Valid, name, address, null);

            if (!_taxSettings.EuVatUseWebService)
                return (VatNumberStatus.Unknown, name, address, null);

            return await DoVatCheck(twoLetterIsoCode, vatNumber);
        }

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        public virtual async Task<(VatNumberStatus status, string name, string address, Exception exception)> 
            DoVatCheck(string twoLetterIsoCode, string vatNumber)
        {
            var name = string.Empty;
            var address = string.Empty;

            if (vatNumber == null)
                vatNumber = string.Empty;
            vatNumber = vatNumber.Trim().Replace(" ", "");

            if (twoLetterIsoCode == null)
                twoLetterIsoCode = string.Empty;
            if (!String.IsNullOrEmpty(twoLetterIsoCode))
                //The service returns INVALID_INPUT for country codes that are not uppercase.
                twoLetterIsoCode = twoLetterIsoCode.ToUpper();

            EuropaCheckVatService.checkVatPortTypeClient s = null;

            try
            {
                s = new EuropaCheckVatService.checkVatPortTypeClient();
                var result = await s.checkVatAsync(new EuropaCheckVatService.checkVatRequest
                {
                    vatNumber = vatNumber,
                    countryCode = twoLetterIsoCode
                });

                var valid = result.valid;
                name = result.name;
                address = result.address;

                return (valid ? VatNumberStatus.Valid : VatNumberStatus.Invalid, name, address, null);
            }
            catch (Exception ex)
            {
                return (VatNumberStatus.Unknown, string.Empty, string.Empty, ex);
            }
            finally
            {
                if (name == null)
                    name = string.Empty;

                if (address == null)
                    address = string.Empty;
            }

        }

        /// <summary>
        /// Gets a value indicating whether a product is tax exempt
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>A value indicating whether a product is tax exempt</returns>
        public virtual bool IsTaxExempt(Product product, Customer customer)
        {
            if (customer != null)
            {
                if (customer.IsTaxExempt)
                    return true;

                if (customer.CustomerRoles.Where(cr => cr.Active).Any(cr => cr.TaxExempt))
                    return true;
            }

            if (product == null)
            {
                return false;
            }

            if (product.IsTaxExempt)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether EU VAT exempt (the European Union Value Added Tax)
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public virtual async Task<bool> IsVatExempt(Address address, Customer customer)
        {
            if (!_taxSettings.EuVatEnabled)
                return false;

            if (address == null || String.IsNullOrEmpty(address.CountryId) || customer == null)
                return false;

            var country = await _countryService.GetCountryById(address.CountryId);
            if (!country.SubjectToVat)
                // VAT not chargeable if shipping outside VAT zone
                return true;

            // VAT not chargeable if address, customer and config meet our VAT exemption requirements:
            // returns true if this customer is VAT exempt because they are shipping within the EU but outside our shop country, they have supplied a validated VAT number, and the shop is configured to allow VAT exemption
            var customerVatStatus = (VatNumberStatus)customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.VatNumberStatusId);
            return address.CountryId != _taxSettings.EuVatShopCountryId &&
                   customerVatStatus == VatNumberStatus.Valid &&
                   _taxSettings.EuVatAllowVatExemption;
        }

        #endregion
    }
}
