using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Customer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface ICustomerViewModelService
    {
        Task DeleteAccount(Customer customer);
        Task<IList<CustomerAttributeModel>> PrepareCustomAttributes(Customer customer,
            string overrideAttributesXml = "");
        Task<CustomerInfoModel> PrepareInfoModel(CustomerInfoModel model, Customer customer,
            bool excludeProperties, string overrideCustomCustomerAttributesXml = "");
        Task<RegisterModel> PrepareRegisterModel(RegisterModel model, bool excludeProperties,
            string overrideCustomCustomerAttributesXml = "");
        Task<string> ParseCustomAttributes(IFormCollection form);

        LoginModel PrepareLogin(bool? checkoutAsGuest);
        PasswordRecoveryModel PreparePasswordRecovery();
        Task<PasswordRecoveryConfirmModel> PreparePasswordRecoveryConfirmModel(Customer customer, string token);
        Task PasswordRecoverySend(PasswordRecoveryModel model, Customer customer);
        Task<CustomerNavigationModel> PrepareNavigation(int selectedTabId = 0);
        Task<CustomerAddressListModel> PrepareAddressList(Customer customer);
        Task<CustomerDownloadableProductsModel> PrepareDownloadableProducts(string customerId);
        Task<UserAgreementModel> PrepareUserAgreement(Guid orderItemId);
        Task<CustomerAvatarModel> PrepareAvatar(Customer customer);
        Task<CustomerAuctionsModel> PrepareAuctions(Customer customer);
        Task<CustomerNotesModel> PrepareNotes(Customer customer);
        Task<DocumentsModel> PrepareDocuments(Customer customer);
        Task<CustomerProductReviewsModel> PrepareReviews(Customer customer);
        Task<CoursesModel> PrepareCourses(Customer customer, Store store);
    }
}