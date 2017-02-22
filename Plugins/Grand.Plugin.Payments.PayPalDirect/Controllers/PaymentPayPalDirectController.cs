using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Plugin.Payments.PayPalDirect.Models;
using Grand.Plugin.Payments.PayPalDirect.Validators;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Services.Common;
using PayPal.Api;
using System.Net;
using System.IO;

namespace Grand.Plugin.Payments.PayPalDirect.Controllers
{
    public class PaymentPayPalDirectController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public PaymentPayPalDirectController(IWorkContext workContext,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger,
            PaymentSettings paymentSettings, 
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._paymentSettings = paymentSettings;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
        }
        #region Utilities

        /// <summary>
        /// Create webhook that receive events for the subscribed event types
        /// </summary>
        /// <returns>Webhook id</returns>
        protected string CreateWebHook()
        {
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalDirectPaymentSettings = _settingService.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            try
            {
                var apiContext = PaypalHelper.GetApiContext(payPalDirectPaymentSettings);
                if (!string.IsNullOrEmpty(payPalDirectPaymentSettings.WebhookId))
                {
                    try
                    {
                        return Webhook.Get(apiContext, payPalDirectPaymentSettings.WebhookId).id;
                    }
                    catch (PayPal.PayPalException) { }
                }

                var currentStore = !string.IsNullOrEmpty(storeScope) ? _storeService.GetStoreById(storeScope) : _storeContext.CurrentStore;
                var webhook = new Webhook
                {
                    event_types = new List<WebhookEventType> { new WebhookEventType { name = "*" } },
                    url = string.Format("{0}Plugins/PaymentPayPalDirect/Webhook", _webHelper.GetStoreLocation(currentStore.SslEnabled))
                }.Create(apiContext);

                return webhook.id;
            }
            catch (PayPal.PayPalException exc)
            {
                if (exc is PayPal.ConnectionException)
                {
                    var error = JsonFormatter.ConvertFromJson<Error>((exc as PayPal.ConnectionException).Response);
                    if (error != null)
                    {
                        _logger.Error(string.Format("PayPal error: {0} ({1})", error.message, error.name));
                        if (error.details != null)
                            error.details.ForEach(x => _logger.Error(string.Format("{0} {1}", x.field, x.issue)));
                    }
                    else
                        _logger.Error(exc.InnerException != null ? exc.InnerException.Message : exc.Message);
                }
                else
                    _logger.Error(exc.InnerException != null ? exc.InnerException.Message : exc.Message);

                return string.Empty;
            }
        }

        #endregion
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalDirectPaymentSettings = _settingService.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ClientId = payPalDirectPaymentSettings.ClientId,
                ClientSecret = payPalDirectPaymentSettings.ClientSecret,
                WebhookId = payPalDirectPaymentSettings.WebhookId,
                UseSandbox = payPalDirectPaymentSettings.UseSandbox,
                PassPurchasedItems = payPalDirectPaymentSettings.PassPurchasedItems,
                TransactModeId = (int)payPalDirectPaymentSettings.TransactMode,
                AdditionalFee = payPalDirectPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = payPalDirectPaymentSettings.AdditionalFeePercentage,
                TransactModeValues = payPalDirectPaymentSettings.TransactMode.ToSelectList(),
                ActiveStoreScopeConfiguration = storeScope
            };
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.ClientId_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.ClientId, storeScope);
                model.ClientSecret_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.ClientSecret, storeScope);
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.UseSandbox, storeScope);
                model.PassPurchasedItems_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.PassPurchasedItems, storeScope);
                model.TransactModeId_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.TransactMode, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(payPalDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            }

            return View("~/Plugins/Payments.PayPalDirect/Views/PaymentPayPalDirect/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalDirectPaymentSettings = _settingService.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            //save settings
            payPalDirectPaymentSettings.ClientId = model.ClientId;
            payPalDirectPaymentSettings.ClientSecret = model.ClientSecret;
            payPalDirectPaymentSettings.WebhookId = model.WebhookId;
            payPalDirectPaymentSettings.UseSandbox = model.UseSandbox;
            payPalDirectPaymentSettings.PassPurchasedItems = model.PassPurchasedItems;
            payPalDirectPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            payPalDirectPaymentSettings.AdditionalFee = model.AdditionalFee;
            payPalDirectPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.UseSandbox_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.TransactModeId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.TransactMode, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.TransactMode, storeScope);

            if (model.ClientId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.ClientId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.ClientId, storeScope);

            if (model.ClientSecret_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.ClientSecret, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.ClientSecret, storeScope);

            if (model.PassPurchasedItems_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.PassPurchasedItems, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.PassPurchasedItems, storeScope);

            if (model.AdditionalFee_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(payPalDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(payPalDirectPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            model.CreditCardTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Visa", Value = "visa" },
                new SelectListItem { Text = "Master card", Value = "MasterCard" },
                new SelectListItem { Text = "Discover", Value = "Discover" },
                new SelectListItem { Text = "Amex", Value = "Amex" },
            };

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = i.ToString("D2"),
                    Value = i.ToString(),
                });
            }

            //set postback values
            model.CardNumber = Request.Form["CardNumber"];
            model.CardCode = Request.Form["CardCode"];
            var selectedCcType = model.CreditCardTypes.FirstOrDefault(x => x.Value.Equals(Request.Form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedCcType != null)
                selectedCcType.Selected = true;
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.PayPalDirect/Views/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            return new ProcessPaymentRequest
            {
                CreditCardType = form["CreditCardType"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = int.Parse(form["ExpireMonth"]),
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"]
            };
        }

        [HttpPost]
        public ActionResult WebhookEventsHandler()
        {
            var storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalDirectPaymentSettings = _settingService.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            try
            {
                var requestBody = string.Empty;
                using (var stream = new StreamReader(Request.InputStream))
                {
                    requestBody = stream.ReadToEnd();
                }
                var apiContext = PaypalHelper.GetApiContext(payPalDirectPaymentSettings);

                //validate request
                if (!WebhookEvent.ValidateReceivedEvent(apiContext, Request.Headers, requestBody, payPalDirectPaymentSettings.WebhookId))
                {
                    _logger.Error("PayPal error: webhook event was not validated");
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }

                var webhook = JsonFormatter.ConvertFromJson<WebhookEvent>(requestBody);

                if (webhook.resource_type.ToLowerInvariant().Equals("sale"))
                {
                    var sale = JsonFormatter.ConvertFromJson<Sale>(webhook.resource.ToString());

                    //recurring payment
                    if (!string.IsNullOrEmpty(sale.billing_agreement_id))
                    {
                        //get agreement
                        var agreement = Agreement.Get(apiContext, sale.billing_agreement_id);
                        var initialOrder = _orderService.GetOrderByGuid(new Guid(agreement.description));
                        if (initialOrder != null)
                        {
                            var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: initialOrder.Id).FirstOrDefault();
                            if (recurringPayment != null)
                            {
                                if (sale.state.ToLowerInvariant().Equals("completed"))
                                {
                                    if (recurringPayment.RecurringPaymentHistory.Count == 0)
                                    {
                                        //first payment
                                        initialOrder.PaymentStatus = PaymentStatus.Paid;
                                        initialOrder.CaptureTransactionId = sale.id;
                                        _orderService.UpdateOrder(initialOrder);

                                        recurringPayment.RecurringPaymentHistory.Add(new RecurringPaymentHistory
                                        {
                                            RecurringPaymentId = recurringPayment.Id,
                                            OrderId = initialOrder.Id,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                        _orderService.UpdateRecurringPayment(recurringPayment);
                                    }
                                    else
                                    {
                                        //next payments
                                        var orders = _orderService.GetOrdersByIds(recurringPayment.RecurringPaymentHistory.Select(order => order.OrderId).ToArray());
                                        if (!orders.Any(order => !string.IsNullOrEmpty(order.CaptureTransactionId)
                                            && order.CaptureTransactionId.Equals(sale.id, StringComparison.InvariantCultureIgnoreCase)))
                                        {
                                            var processPaymentResult = new ProcessPaymentResult
                                            {
                                                NewPaymentStatus = PaymentStatus.Paid,
                                                CaptureTransactionId = sale.id
                                            };
                                            _orderProcessingService.ProcessNextRecurringPayment(recurringPayment);
                                        }
                                    }
                                }
                                else if (sale.state.ToLowerInvariant().Equals("denied"))
                                {
                                    //payment denied
                                    _orderProcessingService.ProcessNextRecurringPayment(recurringPayment);
                                }
                                else
                                    _logger.Error(string.Format("PayPal error: Sale is {0} for the order #{1}", sale.state, initialOrder.Id));
                            }
                        }
                    }
                    else
                    //standard payment
                    {
                        var order = _orderService.GetOrderByGuid(new Guid(sale.invoice_number));
                        if (order != null)
                        {
                            if (sale.state.ToLowerInvariant().Equals("completed"))
                            {
                                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                {
                                    order.CaptureTransactionId = sale.id;
                                    order.CaptureTransactionResult = sale.state;
                                    _orderService.UpdateOrder(order);
                                    _orderProcessingService.MarkOrderAsPaid(order);
                                }
                            }
                            if (sale.state.ToLowerInvariant().Equals("denied"))
                            {
                                var reason = string.Format("Payment is denied. {0}", sale.fmf_details != null ?
                                    string.Format("Based on fraud filter: {0}. {1}", sale.fmf_details.name, sale.fmf_details.description) : string.Empty);

                                _orderService.InsertOrderNote(new OrderNote
                                {
                                    Note = reason,
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id,
                                });

                                _logger.Error(string.Format("PayPal error: {0}", reason));
                            }
                        }
                        else
                            _logger.Error(string.Format("PayPal error: Order with guid {0} was not found", sale.invoice_number));
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (PayPal.PayPalException exc)
            {
                if (exc is PayPal.ConnectionException)
                {
                    var error = JsonFormatter.ConvertFromJson<Error>((exc as PayPal.ConnectionException).Response);
                    if (error != null)
                    {
                        _logger.Error(string.Format("PayPal error: {0} ({1})", error.message, error.name));
                        if (error.details != null)
                            error.details.ForEach(x => _logger.Error(string.Format("{0} {1}", x.field, x.issue)));
                    }
                    else
                        _logger.Error(exc.InnerException != null ? exc.InnerException.Message : exc.Message);
                }
                else
                    _logger.Error(exc.InnerException != null ? exc.InnerException.Message : exc.Message);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
        }
    }
}