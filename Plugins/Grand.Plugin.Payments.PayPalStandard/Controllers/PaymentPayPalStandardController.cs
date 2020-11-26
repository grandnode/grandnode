﻿using Grand.Core;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.PayPalStandard.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Plugin.Payments.PayPalStandard.Controllers
{

    public class PaymentPayPalStandardController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderRecurringPayment _orderRecurringPayment;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly PaymentSettings _paymentSettings;

        public PaymentPayPalStandardController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IOrderRecurringPayment orderRecurringPayment,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            IPermissionService permissionService,
            PaymentSettings paymentSettings)
        {
            _workContext = workContext;
            _storeService = storeService;
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _orderRecurringPayment = orderRecurringPayment;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _logger = logger;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _paymentSettings = paymentSettings;
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UseSandbox = payPalStandardPaymentSettings.UseSandbox;
            model.BusinessEmail = payPalStandardPaymentSettings.BusinessEmail;
            model.PdtToken = payPalStandardPaymentSettings.PdtToken;
            model.PdtValidateOrderTotal = payPalStandardPaymentSettings.PdtValidateOrderTotal;
            model.AdditionalFee = payPalStandardPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = payPalStandardPaymentSettings.AdditionalFeePercentage;
            model.PassProductNamesAndTotals = payPalStandardPaymentSettings.PassProductNamesAndTotals;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (!String.IsNullOrEmpty(storeScope))
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope);
                model.BusinessEmail_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope);
                model.PdtToken_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PdtToken, storeScope);
                model.PdtValidateOrderTotal_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
            }

            return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area("Admin")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            //save settings
            payPalStandardPaymentSettings.UseSandbox = model.UseSandbox;
            payPalStandardPaymentSettings.BusinessEmail = model.BusinessEmail;
            payPalStandardPaymentSettings.PdtToken = model.PdtToken;
            payPalStandardPaymentSettings.PdtValidateOrderTotal = model.PdtValidateOrderTotal;
            payPalStandardPaymentSettings.AdditionalFee = model.AdditionalFee;
            payPalStandardPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            payPalStandardPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.UseSandbox_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.BusinessEmail_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope);

            if (model.PdtToken_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.PdtToken, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.PdtToken, storeScope);

            if (model.PdtValidateOrderTotal_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.PdtValidateOrderTotal, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);

            if (model.AdditionalFee_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.PassProductNamesAndTotals_OverrideForStore || String.IsNullOrEmpty(storeScope))
                await _settingService.SaveSetting(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                await _settingService.DeleteSetting(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);

            //now clear settings cache
            await _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return await Configure();
        }

        public async Task<IActionResult> PDTHandler(IFormCollection form)
        {
            var tx = _webHelper.QueryString<string>("tx");
            Dictionary<string, string> values;
            string response;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard") as PayPalStandardPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new GrandException("PayPal Standard module cannot be loaded");

            if (processor.GetPdtDetails(tx, out values, out response))
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = await _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    decimal mc_gross = decimal.Zero;
                    try
                    {
                        mc_gross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                    }
                    catch (Exception exc)
                    {
                        _logger.Error("PayPal PDT. Error getting mc_gross", exc);
                    }

                    string payer_status = string.Empty;
                    values.TryGetValue("payer_status", out payer_status);
                    string payment_status = string.Empty;
                    values.TryGetValue("payment_status", out payment_status);
                    string pending_reason = string.Empty;
                    values.TryGetValue("pending_reason", out pending_reason);
                    string mc_currency = string.Empty;
                    values.TryGetValue("mc_currency", out mc_currency);
                    string txn_id = string.Empty;
                    values.TryGetValue("txn_id", out txn_id);
                    string payment_type = string.Empty;
                    values.TryGetValue("payment_type", out payment_type);
                    string payer_id = string.Empty;
                    values.TryGetValue("payer_id", out payer_id);
                    string receiver_id = string.Empty;
                    values.TryGetValue("receiver_id", out receiver_id);
                    string invoice = string.Empty;
                    values.TryGetValue("invoice", out invoice);
                    string payment_fee = string.Empty;
                    values.TryGetValue("payment_fee", out payment_fee);

                    var sb = new StringBuilder();
                    sb.AppendLine("Paypal PDT:");
                    sb.AppendLine("mc_gross: " + mc_gross);
                    sb.AppendLine("Payer status: " + payer_status);
                    sb.AppendLine("Payment status: " + payment_status);
                    sb.AppendLine("Pending reason: " + pending_reason);
                    sb.AppendLine("mc_currency: " + mc_currency);
                    sb.AppendLine("txn_id: " + txn_id);
                    sb.AppendLine("payment_type: " + payment_type);
                    sb.AppendLine("payer_id: " + payer_id);
                    sb.AppendLine("receiver_id: " + receiver_id);
                    sb.AppendLine("invoice: " + invoice);
                    sb.AppendLine("payment_fee: " + payment_fee);

                    var newPaymentStatus = PaypalHelper.GetPaymentStatus(payment_status, pending_reason);
                    sb.AppendLine("New payment status: " + newPaymentStatus);

                    //order note
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });

                    //load settings for a chosen store scope
                    var storeScope = await this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                    var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

                    //validate order total
                    if (payPalStandardPaymentSettings.PdtValidateOrderTotal && !Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal * order.CurrencyRate, 2)))
                    {
                        string errorStr = string.Format("PayPal PDT. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal * order.CurrencyRate, order.OrderNumber);
                        _logger.Error(errorStr);

                        //order note
                        await _orderService.InsertOrderNote(new OrderNote {
                            Note = errorStr,
                            OrderId = order.Id,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }

                    //mark order as paid
                    if (newPaymentStatus == PaymentStatus.Paid)
                    {
                        if (await _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.AuthorizationTransactionId = txn_id;
                            await _orderService.UpdateOrder(order);
                            await _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = await _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    //order note
                    await _orderService.InsertOrderNote(new OrderNote {
                        Note = "PayPal PDT failed. " + response,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                    });
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public async Task<IActionResult> IPNHandler()
        {
            string strRequest = string.Empty;
            using (var stream = new StreamReader(Request.Body))
            {
                strRequest = await stream.ReadToEndAsync();
            }
            Dictionary<string, string> values;
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard") as PayPalStandardPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new GrandException("PayPal Standard module cannot be loaded");

            if (processor.VerifyIpn(strRequest, out values))
            {
                #region values
                decimal mc_gross = decimal.Zero;
                try
                {
                    mc_gross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                }
                catch { }

                string payer_status = string.Empty;
                values.TryGetValue("payer_status", out payer_status);
                string payment_status = string.Empty;
                values.TryGetValue("payment_status", out payment_status);
                string pending_reason = string.Empty;
                values.TryGetValue("pending_reason", out pending_reason);
                string mc_currency = string.Empty;
                values.TryGetValue("mc_currency", out mc_currency);
                string txn_id = string.Empty;
                values.TryGetValue("txn_id", out txn_id);
                string txn_type = string.Empty;
                values.TryGetValue("txn_type", out txn_type);
                string rp_invoice_id = string.Empty;
                values.TryGetValue("rp_invoice_id", out rp_invoice_id);
                string payment_type = string.Empty;
                values.TryGetValue("payment_type", out payment_type);
                string payer_id = string.Empty;
                values.TryGetValue("payer_id", out payer_id);
                string receiver_id = string.Empty;
                values.TryGetValue("receiver_id", out receiver_id);
                string invoice = string.Empty;
                values.TryGetValue("invoice", out invoice);
                string payment_fee = string.Empty;
                values.TryGetValue("payment_fee", out payment_fee);

                #endregion

                var sb = new StringBuilder();
                sb.AppendLine("Paypal IPN:");
                foreach (KeyValuePair<string, string> kvp in values)
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
                }

                var newPaymentStatus = PaypalHelper.GetPaymentStatus(payment_status, pending_reason);
                sb.AppendLine("New payment status: " + newPaymentStatus);

                switch (txn_type)
                {
                    case "recurring_payment_profile_created":
                        //do nothing here
                        break;
                    case "recurring_payment":
                        #region Recurring payment
                        {
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(rp_invoice_id);
                            }
                            catch
                            {
                            }

                            var initialOrder = await _orderService.GetOrderByGuid(orderNumberGuid);
                            if (initialOrder != null)
                            {
                                var recurringPayments = await _orderService.SearchRecurringPayments(initialOrderId: initialOrder.Id);
                                foreach (var rp in recurringPayments)
                                {
                                    switch (newPaymentStatus)
                                    {
                                        case PaymentStatus.Authorized:
                                        case PaymentStatus.Paid:
                                            {
                                                var recurringPaymentHistory = rp.RecurringPaymentHistory;
                                                if (recurringPaymentHistory.Count == 0)
                                                {
                                                    //first payment
                                                    var rph = new RecurringPaymentHistory {
                                                        RecurringPaymentId = rp.Id,
                                                        OrderId = initialOrder.Id,
                                                        CreatedOnUtc = DateTime.UtcNow
                                                    };
                                                    rp.RecurringPaymentHistory.Add(rph);
                                                    await _orderService.UpdateRecurringPayment(rp);
                                                }
                                                else
                                                {
                                                    //next payments
                                                    await _orderRecurringPayment.ProcessNextRecurringPayment(rp);
                                                }
                                            }
                                            break;
                                    }
                                }

                                _logger.Information("PayPal IPN. Recurring info", new GrandException(sb.ToString()));
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new GrandException(sb.ToString()));
                            }
                        }
                        #endregion
                        break;
                    default:
                        #region Standard payment
                        {
                            string orderNumber = string.Empty;
                            values.TryGetValue("custom", out orderNumber);
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(orderNumber);
                            }
                            catch
                            {
                            }

                            var order = await _orderService.GetOrderByGuid(orderNumberGuid);
                            if (order != null)
                            {
                                //order note
                                await _orderService.InsertOrderNote(new OrderNote {
                                    Note = sb.ToString(),
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id,
                                });

                                switch (newPaymentStatus)
                                {
                                    case PaymentStatus.Pending:
                                        {
                                        }
                                        break;
                                    case PaymentStatus.Authorized:
                                        {
                                            //validate order total
                                            if (Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal * order.CurrencyRate, 2)))
                                            {
                                                //valid
                                                if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                                                {
                                                    await _orderProcessingService.MarkAsAuthorized(order);
                                                }
                                            }
                                            else
                                            {
                                                //not valid
                                                string errorStr = string.Format("PayPal IPN. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal * order.CurrencyRate, order.Id);
                                                //log
                                                _logger.Error(errorStr);
                                                //order note
                                                await _orderService.InsertOrderNote(new OrderNote {
                                                    Note = errorStr,
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow,
                                                    OrderId = order.Id,
                                                });
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Paid:
                                        {
                                            //validate order total
                                            if (Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal * order.CurrencyRate, 2)))
                                            {
                                                //valid
                                                if (await _orderProcessingService.CanMarkOrderAsPaid(order))
                                                {
                                                    order.AuthorizationTransactionId = txn_id;
                                                    await _orderService.UpdateOrder(order);
                                                    await _orderProcessingService.MarkOrderAsPaid(order);
                                                }
                                            }
                                            else
                                            {
                                                //not valid
                                                string errorStr = string.Format("PayPal IPN. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal * order.CurrencyRate, order.Id);
                                                //log
                                                _logger.Error(errorStr);
                                                //order note
                                                await _orderService.InsertOrderNote(new OrderNote {
                                                    Note = errorStr,
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow,
                                                    OrderId = order.Id,
                                                });
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Refunded:
                                        {
                                            var totalToRefund = Math.Abs(mc_gross);
                                            if (totalToRefund > 0 && Math.Round(totalToRefund, 2).Equals(Math.Round(order.OrderTotal * order.CurrencyRate, 2)))
                                            {
                                                //refund
                                                if (_orderProcessingService.CanRefundOffline(order))
                                                {
                                                    await _orderProcessingService.RefundOffline(order);
                                                }
                                            }
                                            else
                                            {
                                                //partial refund
                                                if (_orderProcessingService.CanPartiallyRefundOffline(order, totalToRefund))
                                                {
                                                    await _orderProcessingService.PartiallyRefundOffline(order, totalToRefund);
                                                }
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Voided:
                                        {
                                            if (_orderProcessingService.CanVoidOffline(order))
                                            {
                                                await _orderProcessingService.VoidOffline(order);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new GrandException(sb.ToString()));
                            }
                        }
                        #endregion
                        break;
                }
            }
            else
            {
                _logger.Error("PayPal IPN failed.", new GrandException(strRequest));
            }

            //nothing should be rendered to visitor
            return Content("");
        }

        public async Task<IActionResult> CancelOrder(IFormCollection form)
        {
            var order = (await _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1)).FirstOrDefault();
            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("HomePage");
        }
    }
}