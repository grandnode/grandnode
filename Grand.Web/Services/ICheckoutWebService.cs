using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Payments;
using Grand.Web.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Services
{
    public partial interface ICheckoutWebService
    {
        bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null);
        CheckoutBillingAddressModel PrepareBillingAddress(
            IList<ShoppingCartItem> cart, string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");
        CheckoutShippingAddressModel PrepareShippingAddress(string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");
        CheckoutShippingMethodModel PrepareShippingMethod(IList<ShoppingCartItem> cart, Address shippingAddress);
        CheckoutPaymentMethodModel PreparePaymentMethod(IList<ShoppingCartItem> cart, string filterByCountryId);
        CheckoutPaymentInfoModel PreparePaymentInfo(IPaymentMethod paymentMethod);
        CheckoutConfirmModel PrepareConfirmOrder(IList<ShoppingCartItem> cart);
        bool IsMinimumOrderPlacementIntervalValid(Customer customer);
    }
}