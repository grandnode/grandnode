using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Payments;
using Grand.Web.Models.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICheckoutViewModelService
    {
        Task<bool> IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null);
        Task<CheckoutBillingAddressModel> PrepareBillingAddress(
            IList<ShoppingCartItem> cart, string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");
        Task<CheckoutShippingAddressModel> PrepareShippingAddress(string selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "");
        Task<CheckoutShippingMethodModel> PrepareShippingMethod(IList<ShoppingCartItem> cart, Address shippingAddress);
        Task<CheckoutPaymentMethodModel> PreparePaymentMethod(IList<ShoppingCartItem> cart, string filterByCountryId);
        CheckoutPaymentInfoModel PreparePaymentInfo(IPaymentMethod paymentMethod);
        Task<CheckoutConfirmModel> PrepareConfirmOrder(IList<ShoppingCartItem> cart);
        Task<bool> IsMinimumOrderPlacementIntervalValid(Customer customer);
    }
}