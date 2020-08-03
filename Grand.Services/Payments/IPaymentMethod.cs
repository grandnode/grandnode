using Grand.Domain.Orders;
using Grand.Core.Plugins;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Payments
{
    /// <summary>
    /// Provides an interface for creating payment gateways & methods
    /// </summary>
    public partial interface IPaymentMethod : IPlugin
    {
        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        Task<ProcessPaymentResult> ProcessPayment(ProcessPaymentRequest processPaymentRequest);
        
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        Task PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        Task<decimal> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        Task<CapturePaymentResult> Capture(CapturePaymentRequest capturePaymentRequest);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest);

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        Task<VoidPaymentResult> Void(VoidPaymentRequest voidPaymentRequest);

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        Task<ProcessPaymentResult> ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        Task<CancelRecurringPaymentResult> CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest);

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        Task<bool> CanRePostProcessPayment(Order order);

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        Task<IList<string>> ValidatePaymentForm(IFormCollection form);

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        Task<ProcessPaymentRequest> GetPaymentInfo(IFormCollection form);

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        void GetPublicViewComponent(out string viewComponentName);

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        Task<bool> SupportCapture();

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        Task<bool> SupportPartiallyRefund();

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        Task<bool> SupportRefund();

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        Task<bool> SupportVoid();

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        RecurringPaymentType RecurringPaymentType { get; }
        
        /// <summary>
        /// Gets a payment method type
        /// </summary>
        PaymentMethodType PaymentMethodType { get; }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        Task<bool> SkipPaymentInfo();

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        Task<string> PaymentMethodDescription();
        #endregion
    }
}
