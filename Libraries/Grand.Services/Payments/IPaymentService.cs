using System.Collections.Generic;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Customers;

namespace Grand.Services.Payments
{
    /// <summary>
    /// Payment service interface
    /// </summary>
    public partial interface IPaymentService
    {
        /// <summary>
        /// Load active payment methods
        /// </summary>
        /// <param name="filterByCustomerId">Filter payment methods by customer; null to load all records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="filterByCountryId">Load records allowed only in a specified country; pass 0 to load all records</param>
        /// <returns>Payment methods</returns>
        IList<IPaymentMethod> LoadActivePaymentMethods(Customer filterByCustomer = null, string storeId = "", string filterByCountryId = "");

        /// <summary>
        /// Load payment provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found payment provider</returns>
        IPaymentMethod LoadPaymentMethodBySystemName(string systemName);

        /// <summary>
        /// Load all payment providers
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="filterByCountryId">Load records allowed only in a specified country; pass 0 to load all records</param>
        /// <returns>Payment providers</returns>
        IList<IPaymentMethod> LoadAllPaymentMethods(string storeId = "", string filterByCountryId = "");

        /// <summary>
        /// Gets a list of coutnry identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of country identifiers</returns>
        IList<string> GetRestictedCountryIds(IPaymentMethod paymentMethod);

        /// <summary>
        /// Gets a list of role identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of role identifiers</returns>
        IList<string> GetRestictedRoleIds(IPaymentMethod paymentMethod);

        /// <summary>
        /// Gets a list of shipping identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of role identifiers</returns>
        IList<string> GetRestictedShippingIds(IPaymentMethod paymentMethod);

        /// <summary>
        /// Saves a list of country identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="countryIds">A list of country identifiers</param>
        void SaveRestictedCountryIds(IPaymentMethod paymentMethod, List<string> countryIds);

        /// <summary>
        /// Saves a list of role identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="roleIds">A list of role identifiers</param>
        void SaveRestictedRoleIds(IPaymentMethod paymentMethod, List<string> roleIds);

        /// <summary>
        /// Saves a list of shipping identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="shippingIds">A list of shipping identifiers</param>
        void SaveRestictedShippingIds(IPaymentMethod paymentMethod, List<string> shippingIds);

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        bool CanRePostProcessPayment(Order order);


        /// <summary>
        /// Gets an additional handling fee of a payment method
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Additional handling fee</returns>
        decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart, string paymentMethodSystemName);



        /// <summary>
        /// Gets a value indicating whether capture is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether capture is supported</returns>
        bool SupportCapture(string paymentMethodSystemName);

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest);



        /// <summary>
        /// Gets a value indicating whether partial refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether partial refund is supported</returns>
        bool SupportPartiallyRefund(string paymentMethodSystemName);

        /// <summary>
        /// Gets a value indicating whether refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether refund is supported</returns>
        bool SupportRefund(string paymentMethodSystemName);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);



        /// <summary>
        /// Gets a value indicating whether void is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether void is supported</returns>
        bool SupportVoid(string paymentMethodSystemName);

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest);



        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A recurring payment type of payment method</returns>
        RecurringPaymentType GetRecurringPaymentType(string paymentMethodSystemName);

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest);



        /// <summary>
        /// Gets a payment method type
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A payment method type</returns>
        PaymentMethodType GetPaymentMethodType(string paymentMethodSystemName);

        /// <summary>
        /// Gets masked credit card number
        /// </summary>
        /// <param name="creditCardNumber">Credit card number</param>
        /// <returns>Masked credit card number</returns>
        string GetMaskedCreditCardNumber(string creditCardNumber);
        
    }
}
