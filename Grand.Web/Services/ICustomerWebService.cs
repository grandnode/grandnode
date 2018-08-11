using Grand.Core.Domain.Customers;
using Grand.Web.Models.Customer;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Services
{
    public partial interface ICustomerWebService
    {
        void DeleteAccount(Customer customer);
        IList<CustomerAttributeModel> PrepareCustomAttributes(Customer customer,
            string overrideAttributesXml = "");
        CustomerInfoModel PrepareInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "");
        RegisterModel PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "");
        string ParseCustomAttributes(IFormCollection form);

        LoginModel PrepareLogin(bool? checkoutAsGuest);
        PasswordRecoveryModel PreparePasswordRecovery();
        PasswordRecoveryConfirmModel PreparePasswordRecoveryConfirmModel(Customer customer, string token);
        void PasswordRecoverySend(PasswordRecoveryModel model, Customer customer);
        CustomerNavigationModel PrepareNavigation(int selectedTabId = 0);
        CustomerAddressListModel PrepareAddressList(Customer customer);
        CustomerDownloadableProductsModel PrepareDownloadableProducts(string customerId);
        UserAgreementModel PrepareUserAgreement(Guid orderItemId);
        CustomerAvatarModel PrepareAvatar(Customer customer);
        CustomerAuctionsModel PrepareAuctions(Customer customer);
    }
}