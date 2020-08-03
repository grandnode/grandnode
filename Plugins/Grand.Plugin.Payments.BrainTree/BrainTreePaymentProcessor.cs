using Braintree;
using Grand.Core;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Core.Plugins;
using Grand.Plugin.Payments.BrainTree.Models;
using Grand.Plugin.Payments.BrainTree.Validators;
using Grand.Services.Cms;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Environment = Braintree.Environment;

namespace Grand.Plugin.Payments.BrainTree
{
    public class BrainTreePaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly BrainTreePaymentSettings _brainTreePaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ILanguageService _languageService;

        #endregion

        #region Ctor

        public BrainTreePaymentProcessor(ICustomerService customerService,
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            BrainTreePaymentSettings brainTreePaymentSettings,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ILanguageService languageService)
        {
            _customerService = customerService;
            _settingService = settingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _brainTreePaymentSettings = brainTreePaymentSettings;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _languageService = languageService;
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
            var processPaymentResult = new ProcessPaymentResult();

            //get customer
            var customer = await _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            //get settings
            var useSandBox = _brainTreePaymentSettings.UseSandBox;
            var merchantId = _brainTreePaymentSettings.MerchantId;
            var publicKey = _brainTreePaymentSettings.PublicKey;
            var privateKey = _brainTreePaymentSettings.PrivateKey;

            //new gateway
            var gateway = new BraintreeGateway {
                Environment = useSandBox ? Environment.SANDBOX : Environment.PRODUCTION,
                MerchantId = merchantId,
                PublicKey = publicKey,
                PrivateKey = privateKey
            };

            //new transaction request
            var transactionRequest = new TransactionRequest {
                Amount = processPaymentRequest.OrderTotal,
            };

            if (_brainTreePaymentSettings.Use3DS)
            {
                transactionRequest.PaymentMethodNonce = processPaymentRequest.CustomValues["CardNonce"].ToString();
            }
            else
            {
                //transaction credit card request
                var transactionCreditCardRequest = new TransactionCreditCardRequest {
                    Number = processPaymentRequest.CreditCardNumber,
                    CVV = processPaymentRequest.CreditCardCvv2,
                    ExpirationDate = processPaymentRequest.CreditCardExpireMonth + "/" + processPaymentRequest.CreditCardExpireYear
                };
                transactionRequest.CreditCard = transactionCreditCardRequest;
            }

            //address request
            var addressRequest = new AddressRequest {
                FirstName = customer.BillingAddress.FirstName,
                LastName = customer.BillingAddress.LastName,
                StreetAddress = customer.BillingAddress.Address1,
                PostalCode = customer.BillingAddress.ZipPostalCode
            };
            transactionRequest.BillingAddress = addressRequest;

            //transaction options request
            var transactionOptionsRequest = new TransactionOptionsRequest {
                SubmitForSettlement = true
            };
            transactionRequest.Options = transactionOptionsRequest;

            //sending a request
            var result = gateway.Transaction.Sale(transactionRequest);

            //result
            if (result.IsSuccess())
            {
                processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                processPaymentResult.AddError("Error processing payment." + result.Message);
            }

            return processPaymentResult;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public Task PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public async Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            //return false;
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            if (_brainTreePaymentSettings.AdditionalFee <= 0)
                return _brainTreePaymentSettings.AdditionalFee;

            decimal result;
            if (_brainTreePaymentSettings.AdditionalFeePercentage)
            {
                //percentage
                var subtotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, true);
                result = (decimal)((((float)subtotal.subTotalWithDiscount) * ((float)_brainTreePaymentSettings.AdditionalFee)) / 100f);
            }
            else
            {
                //fixed value
                result = _brainTreePaymentSettings.AdditionalFee;
            }
            //return result;
            return await Task.FromResult(result);
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

            //it's not a redirection payment method. So we always return false
            return await Task.FromResult(false);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentBrainTree/Configure";
        }

        public async Task<IList<string>> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_brainTreePaymentSettings, _localizationService);
            var model = new PaymentInfoModel {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"],
                CardNonce = form["CardNonce"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                foreach (var error in validationResult.Errors)
                {
                    warnings.Add(error.ErrorMessage);
                }
            return await Task.FromResult(warnings);
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = _brainTreePaymentSettings.Use3DS ? new ProcessPaymentRequest() : new ProcessPaymentRequest {
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };

            if (form.TryGetValue("CardNonce", out var cardNonce) && !StringValues.IsNullOrEmpty(cardNonce))
                paymentInfo.CustomValues.Add("CardNonce", cardNonce.ToString());


            return await Task.FromResult(paymentInfo);
        }

        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentBrainTree";
        }

        public override async Task Install()
        {
            //settings
            var settings = new BrainTreePaymentSettings {
                UseSandBox = true,
                MerchantId = "",
                PrivateKey = "",
                PublicKey = ""
            };
            await _settingService.SaveSetting(settings);

            //locales
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.Use3DS", "Use the 3D secure");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.Use3DS.Hint", "Check to enable the 3D secure integration");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.UseSandbox", "Use Sandbox");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.MerchantId", "Merchant ID");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.MerchantId.Hint", "Enter Merchant ID");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PublicKey", "Public Key");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PublicKey.Hint", "Enter Public key");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PrivateKey", "Private Key");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PrivateKey.Hint", "Enter Private key");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFee", "Additional fee");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.PaymentMethodDescription", "Pay by credit / debit card");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            //settings
            await _settingService.DeleteSetting<BrainTreePaymentSettings>();

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.UseSandbox");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.UseSandbox.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.MerchantId");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.MerchantId.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PublicKey");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PublicKey.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PrivateKey");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.PrivateKey.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFee");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFee.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFeePercentage");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.Fields.AdditionalFeePercentage.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Payments.BrainTree.PaymentMethodDescription");

            await base.Uninstall();
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
            get {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType {
            get {
                return PaymentMethodType.Standard;
            }
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
            return await Task.FromResult(_localizationService.GetResource("Plugins.Payments.BrainTree.PaymentMethodDescription"));
        }

        public IList<string> GetWidgetZones()
        {
            return new string[] { "opc_content_before", "checkout_payment_info_top" };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "PaymentBrainTreeScripts";
        }

        #endregion

    }
}
