using System;
using System.Text;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Payments;
using PayPal.Api;
using System.Collections.Generic;

namespace Grand.Plugin.Payments.PayPalDirect
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class PaypalHelper
    {
        #region Methods

        /// <summary>
        /// Get PayPal Api context 
        /// </summary>
        /// <param name="paypalDirectPaymentSettings">PayPalDirectPayment settings</param>
        /// <returns>ApiContext</returns>
        public static APIContext GetApiContext(PayPalDirectPaymentSettings payPalDirectPaymentSettings)
        {
            var mode = payPalDirectPaymentSettings.UseSandbox ? "sandbox" : "live";

            var config = new Dictionary<string, string>
            {
                { "clientId", payPalDirectPaymentSettings.ClientId },
                { "clientSecret", payPalDirectPaymentSettings.ClientSecret },
                { "mode", mode }
            };

            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken) { Config = config };

            if (apiContext.HTTPHeaders == null)
                apiContext.HTTPHeaders = new Dictionary<string, string>();

            return apiContext;
        }

        #endregion
    }
}

