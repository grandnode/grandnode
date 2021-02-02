using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Services.Orders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Payments
{
    /// <summary>
    /// Payment extensions
    /// </summary>
    public static class PaymentExtensions
    {
        /// <summary>
        /// Is payment method active?
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <returns>Result</returns>
        public static bool IsPaymentMethodActive(this IPaymentMethod paymentMethod,
            PaymentSettings paymentSettings)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            if (paymentSettings == null)
                throw new ArgumentNullException("paymentSettings");

            if (paymentSettings.ActivePaymentMethodSystemNames == null)
                return false;
            foreach (var activeMethodSystemName in paymentSettings.ActivePaymentMethodSystemNames)
                if (paymentMethod.PluginDescriptor.SystemName.Equals(activeMethodSystemName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        /// <summary>
        /// Calculate payment method fee
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="orderTotalCalculationService">Order total calculation service</param>
        /// <param name="cart">Shopping cart</param>
        /// <param name="fee">Fee value</param>
        /// <param name="usePercentage">Is fee amount specified as percentage or fixed value?</param>
        /// <returns>Result</returns>
        public static async Task<decimal> CalculateAdditionalFee(this IPaymentMethod paymentMethod,
            IOrderTotalCalculationService orderTotalCalculationService, IList<ShoppingCartItem> cart,
            decimal fee, bool usePercentage)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");
            if (fee <= 0)
                return fee;

            decimal result;
            if (usePercentage)
            {
                //percentage
                var shoppingCartSubTotal = await orderTotalCalculationService.GetShoppingCartSubTotal(cart, true);
                result = (decimal)((((float)shoppingCartSubTotal.subTotalWithDiscount) * ((float)fee)) / 100f);
            }
            else
            {
                //fixed value
                result = fee;
            }
            return result;
        }


        /// <summary>
        /// Serialize CustomValues of ProcessPaymentRequest
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Serialized CustomValues</returns>
        public static string SerializeCustomValues(this ProcessPaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (!request.CustomValues.Any())
                return null;

            return JsonConvert.SerializeObject(request.CustomValues);
        }
        /// <summary>
        /// Deerialize CustomValues of Order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Serialized CustomValues CustomValues</returns>
        public static Dictionary<string, object> DeserializeCustomValues(this Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var request = new ProcessPaymentRequest();
            return request.DeserializeCustomValues(order.CustomValuesXml);
        }
        /// <summary>
        /// Deerialize CustomValues of ProcessPaymentRequest
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="customValues">Serialized CustomValues</param>
        /// <returns>Serialized CustomValues CustomValues</returns>
        public static Dictionary<string, object> DeserializeCustomValues(this ProcessPaymentRequest _, string customValues)
        {
            if (string.IsNullOrWhiteSpace(customValues))
            {
                return new Dictionary<string, object>();
            }
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(customValues);
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
    }
}
