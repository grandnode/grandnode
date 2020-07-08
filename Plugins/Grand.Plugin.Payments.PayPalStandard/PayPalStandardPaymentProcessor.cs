using Grand.Core;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Tax;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.PayPalStandard
{
    /// <summary>
    /// PayPalStandard payment processor
    /// </summary>
    public class PayPalStandardPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IProductService _productService;
        private readonly IWebHelper _webHelper;
        private readonly PayPalStandardPaymentSettings _paypalStandardPaymentSettings;
        private readonly ILanguageService _languageService;
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Ctor

        public PayPalStandardPaymentProcessor(
            ICheckoutAttributeParser checkoutAttributeParser,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            ITaxService taxService,
            IProductService productService,
            IWebHelper webHelper,
            ILanguageService languageService,
            IServiceProvider serviceProvider,
            PayPalStandardPaymentSettings paypalStandardPaymentSettings)
        {
            _checkoutAttributeParser = checkoutAttributeParser;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _taxService = taxService;
            _productService = productService;
            _webHelper = webHelper;
            _languageService = languageService;
            _serviceProvider = serviceProvider;
            _paypalStandardPaymentSettings = paypalStandardPaymentSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets PayPal URL
        /// </summary>
        /// <returns></returns>
        private string GetPaypalUrl()
        {
            return _paypalStandardPaymentSettings.UseSandbox ?
                "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
                "https://www.paypal.com/us/cgi-bin/webscr";
        }

        /// <summary>
        /// Gets IPN PayPal URL
        /// </summary>
        /// <returns></returns>
        private string GetIpnPaypalUrl()
        {
            return _paypalStandardPaymentSettings.UseSandbox ?
                "https://ipnpb.sandbox.paypal.com/cgi-bin/webscr" :
                "https://ipnpb.paypal.com/cgi-bin/webscr";
        }

        /// <summary>
        /// Gets PDT details
        /// </summary>
        /// <param name="tx">TX</param>
        /// <param name="values">Values</param>
        /// <param name="response">Response</param>
        /// <returns>Result</returns>
        public bool GetPdtDetails(string tx, out Dictionary<string, string> values, out string response)
        {
            var req = (HttpWebRequest)WebRequest.Create(GetPaypalUrl());
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/x-www-form-urlencoded";
            //now PayPal requires user-agent. otherwise, we can get 403 error
            req.UserAgent = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent];

            var formContent = $"cmd=_notify-synch&at={_paypalStandardPaymentSettings.PdtToken}&tx={tx}";
            req.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
                sw.Write(formContent);

            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
                response = WebUtility.UrlDecode(sr.ReadToEnd());

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool firstLine = true, success = false;
            foreach (var l in response.Split('\n'))
            {
                var line = l.Trim();
                if (firstLine)
                {
                    success = line.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
                    firstLine = false;
                }
                else
                {
                    var equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }
            }

            return success;
        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <param name="values">Values</param>
        /// <returns>Result</returns>
        public bool VerifyIpn(string formString, out Dictionary<string, string> values)
        {
            var req = (HttpWebRequest)WebRequest.Create(GetIpnPaypalUrl());
            req.Method = WebRequestMethods.Http.Post;
            req.ContentType = "application/x-www-form-urlencoded";
            //now PayPal requires user-agent. otherwise, we can get 403 error
            req.UserAgent = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent];

            var formContent = $"cmd=_notify-validate&{formString}";
            req.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(formContent);
            }

            string response;
            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                response = WebUtility.UrlDecode(sr.ReadToEnd());
            }
            var success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in formString.Split('&'))
            {
                var line = l.Trim();
                var equalPox = line.IndexOf('=');
                if (equalPox >= 0)
                    values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
            }

            return success;
        }

        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created query parameters</returns>
        private async Task<IDictionary<string, string>> CreateQueryParameters(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //get store location
            var storeLocation = _webHelper.GetStoreLocation();
            var stateProvince = "";
            var countryCode = "";
            if (!String.IsNullOrEmpty(postProcessPaymentRequest.Order.ShippingAddress?.StateProvinceId))
            {
                var state = (await _serviceProvider.GetRequiredService<IStateProvinceService>().GetStateProvinceById(postProcessPaymentRequest.Order.ShippingAddress?.StateProvinceId));
                if (state != null)
                    stateProvince = state.Abbreviation;
            }
            if (!String.IsNullOrEmpty(postProcessPaymentRequest.Order.ShippingAddress?.CountryId))
            {
                var country = await _serviceProvider.GetRequiredService<ICountryService>().GetCountryById(postProcessPaymentRequest.Order.ShippingAddress?.CountryId);
                if (country != null)
                    countryCode = country.TwoLetterIsoCode;
            }


            //create query parameters
            return new Dictionary<string, string> {
                //PayPal ID or an email address associated with your PayPal account
                ["business"] = _paypalStandardPaymentSettings.BusinessEmail,

                //the character set and character encoding
                ["charset"] = "utf-8",

                //set return method to "2" (the customer redirected to the return URL by using the POST method, and all payment variables are included)
                ["rm"] = "2",

                ["currency_code"] = postProcessPaymentRequest.Order.CustomerCurrencyCode,

                //order identifier
                ["invoice"] = postProcessPaymentRequest.Order.OrderNumber.ToString(),
                ["custom"] = postProcessPaymentRequest.Order.OrderGuid.ToString(),

                //PDT, IPN and cancel URL
                ["return"] = $"{storeLocation}Plugins/PaymentPayPalStandard/PDTHandler",
                ["notify_url"] = $"{storeLocation}Plugins/PaymentPayPalStandard/IPNHandler",
                ["cancel_return"] = $"{storeLocation}Plugins/PaymentPayPalStandard/CancelOrder",

                //shipping address, if exists
                ["no_shipping"] = postProcessPaymentRequest.Order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "1" : "2",
                ["address_override"] = postProcessPaymentRequest.Order.ShippingStatus == ShippingStatus.ShippingNotRequired ? "0" : "1",
                ["first_name"] = postProcessPaymentRequest.Order.ShippingAddress?.FirstName,
                ["last_name"] = postProcessPaymentRequest.Order.ShippingAddress?.LastName,
                ["address1"] = postProcessPaymentRequest.Order.ShippingAddress?.Address1,
                ["address2"] = postProcessPaymentRequest.Order.ShippingAddress?.Address2,
                ["city"] = postProcessPaymentRequest.Order.ShippingAddress?.City,

                ["state"] = stateProvince,
                ["country"] = countryCode,
                ["zip"] = postProcessPaymentRequest.Order.ShippingAddress?.ZipPostalCode,
                ["email"] = postProcessPaymentRequest.Order.ShippingAddress?.Email
            };
        }

        /// <summary>
        /// Add order items to the request query parameters
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        private async Task AddItemsParameters(IDictionary<string, string> parameters, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //upload order items
            parameters.Add("cmd", "_cart");
            parameters.Add("upload", "1");

            var cartTotal = decimal.Zero;
            var roundedCartTotal = decimal.Zero;
            var itemCount = 1;

            var rate = postProcessPaymentRequest.Order.CurrencyRate;

            //add shopping cart items
            foreach (var item in postProcessPaymentRequest.Order.OrderItems)
            {
                var product = await _productService.GetProductById(item.ProductId);

                var roundedItemPrice = Math.Round(item.UnitPriceExclTax * rate, 2);

                //add query parameters
                parameters.Add($"item_name_{itemCount}", product.Name);
                parameters.Add($"amount_{itemCount}", roundedItemPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", item.Quantity.ToString());

                cartTotal += (item.PriceExclTax * rate);
                roundedCartTotal += roundedItemPrice * item.Quantity;
                itemCount++;
            }

            //add checkout attributes as order items
            var checkoutAttributeValues = await _checkoutAttributeParser.ParseCheckoutAttributeValue(postProcessPaymentRequest.Order.CheckoutAttributesXml);
            var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(postProcessPaymentRequest.Order.CustomerId);
            foreach (var attributeValue in checkoutAttributeValues)
            {
                var attributePrice = await _taxService.GetCheckoutAttributePrice(attributeValue.ca, attributeValue.cav, false, customer);
                if (attributePrice.checkoutPrice > 0)
                {
                    var roundedAttributePrice = Math.Round(attributePrice.checkoutPrice * rate, 2);
                    //add query parameters
                    if (attributeValue.ca != null)
                    {
                        parameters.Add($"item_name_{itemCount}", attributeValue.ca.Name);
                        parameters.Add($"amount_{itemCount}", roundedAttributePrice.ToString("0.00", CultureInfo.InvariantCulture));
                        parameters.Add($"quantity_{itemCount}", "1");

                        cartTotal += attributePrice.checkoutPrice;
                        roundedCartTotal += roundedAttributePrice;
                        itemCount++;
                    }
                }
            }

            //add shipping fee as a separate order item, if it has price
            var roundedShippingPrice = Math.Round(postProcessPaymentRequest.Order.OrderShippingExclTax * rate, 2);
            if (roundedShippingPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Shipping fee");
                parameters.Add($"amount_{itemCount}", roundedShippingPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += (postProcessPaymentRequest.Order.OrderShippingExclTax * rate);
                roundedCartTotal += roundedShippingPrice;
                itemCount++;
            }

            //add payment method additional fee as a separate order item, if it has price
            var roundedPaymentMethodPrice = Math.Round(postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax * rate, 2);
            if (roundedPaymentMethodPrice > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Payment method fee");
                parameters.Add($"amount_{itemCount}", roundedPaymentMethodPrice.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += (postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax * rate);
                roundedCartTotal += roundedPaymentMethodPrice;
                itemCount++;
            }

            //add tax as a separate order item, if it has positive amount
            var roundedTaxAmount = Math.Round(postProcessPaymentRequest.Order.OrderTax * rate, 2);
            if (roundedTaxAmount > decimal.Zero)
            {
                parameters.Add($"item_name_{itemCount}", "Tax amount");
                parameters.Add($"amount_{itemCount}", roundedTaxAmount.ToString("0.00", CultureInfo.InvariantCulture));
                parameters.Add($"quantity_{itemCount}", "1");

                cartTotal += (postProcessPaymentRequest.Order.OrderTax * rate);
                roundedCartTotal += roundedTaxAmount;
                itemCount++;
            }

            if (cartTotal * rate > postProcessPaymentRequest.Order.OrderTotal * rate)
            {
                //get the difference between what the order total is and what it should be and use that as the "discount"
                var discountTotal = Math.Round(cartTotal - (postProcessPaymentRequest.Order.OrderTotal * rate), 2);
                roundedCartTotal -= discountTotal;

                //gift card or rewarded point amount applied to cart in nopCommerce - shows in PayPal as "discount"
                parameters.Add("discount_amount_cart", discountTotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            //save order total that actually sent to PayPal (used for PDT order total validation)
            await _genericAttributeService.SaveAttribute(postProcessPaymentRequest.Order, PaypalHelper.OrderTotalSentToPayPal, roundedCartTotal);
        }

        /// <summary>
        /// Add order total to the request query parameters
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        private async Task AddOrderTotalParameters(IDictionary<string, string> parameters, PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //round order total
            var roundedOrderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal * postProcessPaymentRequest.Order.CurrencyRate, 2);

            parameters.Add("cmd", "_xclick");
            parameters.Add("item_name", $"Order Number {postProcessPaymentRequest.Order.OrderNumber.ToString()}");
            parameters.Add("amount", roundedOrderTotal.ToString("0.00", CultureInfo.InvariantCulture));

            //save order total that actually sent to PayPal (used for PDT order total validation)
            await _genericAttributeService.SaveAttribute(postProcessPaymentRequest.Order, PaypalHelper.OrderTotalSentToPayPal, roundedOrderTotal);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public async Task PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //create common query parameters for the request
            var queryParameters = await CreateQueryParameters(postProcessPaymentRequest);

            //whether to include order items in a transaction
            if (_paypalStandardPaymentSettings.PassProductNamesAndTotals)
            {
                //add order items query parameters to the request
                var parameters = new Dictionary<string, string>(queryParameters);
                await AddItemsParameters(parameters, postProcessPaymentRequest);

                //remove null values from parameters
                parameters = parameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
                    .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

                //ensure redirect URL doesn't exceed 2K chars to avoid "too long URL" exception
                var redirectUrl = QueryHelpers.AddQueryString(GetPaypalUrl(), parameters);
                if (redirectUrl.Length <= 2048)
                {
                    _httpContextAccessor.HttpContext.Response.Redirect(redirectUrl);
                    return;
                }
            }

            //or add only an order total query parameters to the request
            await AddOrderTotalParameters(queryParameters, postProcessPaymentRequest);

            //remove null values from parameters
            queryParameters = queryParameters.Where(parameter => !string.IsNullOrEmpty(parameter.Value))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

            var url = QueryHelpers.AddQueryString(GetPaypalUrl(), queryParameters);
            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public async Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return await this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _paypalStandardPaymentSettings.AdditionalFee, _paypalStandardPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public async Task<CapturePaymentResult> Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<VoidPaymentResult> Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<CancelRecurringPaymentResult> CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public async Task<bool> CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public async Task<IList<string>> ValidatePaymentForm(IFormCollection form)
        {
            return await Task.FromResult(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public async Task<ProcessPaymentRequest> GetPaymentInfo(IFormCollection form)
        {
            return await Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayPalStandard/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override async Task Install()
        {
            //settings
            await _settingService.SaveSetting(new PayPalStandardPaymentSettings {
                UseSandbox = true
            });

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFee", "Additional fee");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.BusinessEmail", "Business Email");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.BusinessEmail.Hint", "Specify your PayPal business email.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals", "Pass product names and order totals to PayPal");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals.Hint", "Check if product names and order totals should be passed to PayPal.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PDTToken", "PDT Identity Token");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PDTValidateOrderTotal", "PDT. Validate order total");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PDTValidateOrderTotal.Hint", "Check if PDT handler should validate order totals.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.RedirectionTip", "You will be redirected to PayPal site to complete the order.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.UseSandbox", "Use Sandbox");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Instructions", "<p><b>If you're using this gateway ensure that your primary store currency is supported by PayPal.</b><br /><br />To use PDT, you must activate PDT and Auto Return in your PayPal account profile. You must also acquire a PDT identity token, which is used in all PDT communication you send to PayPal. Follow these steps to configure your account for PDT:<br /><br />1. Log in to your PayPal account (click <a href=\"https://www.paypal.com/us/webapps/mpp/referral/paypal-business-account2?partner_id=9JJPJNNPQ7PZ8\" target=\"_blank\">here</a> to create your account).<br />2. Click the Profile subtab.<br />3. Click Website Payment Preferences in the Seller Preferences column.<br />4. Under Auto Return for Website Payments, click the On radio button.<br />5. For the Return URL, enter the URL on your site that will receive the transaction ID posted by PayPal after a customer payment ({0}).<br />6. Under Payment Data Transfer, click the On radio button.<br />7. Click Save.<br />8. Click Website Payment Preferences in the Seller Preferences column.<br />9. Scroll down to the Payment Data Transfer section of the page to view your PDT identity token.<br /><br /></p>");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.PaymentMethodDescription", "You will be redirected to PayPal site to complete the payment");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.RoundingWarning", "It looks like you have \"ShoppingCartSettings.RoundPricesDuringCalculation\" setting disabled. Keep in mind that this can lead to a discrepancy of the order total amount, as PayPal only rounds to two decimals.");

            await base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<PayPalStandardPaymentSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFee");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFee.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.BusinessEmail");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.BusinessEmail.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PDTToken");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.PDTToken.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.RedirectionTip");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.UseSandbox");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Fields.UseSandbox.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.Instructions");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.PaymentMethodDescription");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.PayPalStandard.RoundingWarning");

            await base.Uninstall();
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayPalStandard";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public async Task<bool> SupportCapture()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public async Task<bool> SupportPartiallyRefund()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public async Task<bool> SupportRefund()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public async Task<bool> SupportVoid()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType {
            get { return PaymentMethodType.Redirection; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public async Task<bool> SkipPaymentInfo()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public async Task<string> PaymentMethodDescription()
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            return await Task.FromResult(_localizationService.GetResource("Plugins.Payments.PayPalStandard.PaymentMethodDescription"));
        }

        #endregion
    }
}