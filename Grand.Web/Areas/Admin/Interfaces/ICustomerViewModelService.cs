using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Models.ShoppingCart;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerViewModelService
    {
        Task<CustomerListModel> PrepareCustomerListModel();
        Task<(IEnumerable<CustomerModel> customerModelList, int totalCount)> PrepareCustomerList(CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds, int pageIndex, int pageSize);
        Task PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties);
        string ValidateCustomerRoles(IList<CustomerRole> customerRoles);
        Task<Customer> InsertCustomerModel(CustomerModel model);
        Task<Customer> UpdateCustomerModel(Customer customer, CustomerModel model);
        Task DeleteCustomer(Customer customer);
        Task DeleteSelected(IList<string> selectedIds);
        Task SendEmail(Customer customer, CustomerModel.SendEmailModel model);
        Task SendPM(Customer customer, CustomerModel.SendPmModel model);
        Task<IEnumerable<CustomerModel.RewardPointsHistoryModel>> PrepareRewardPointsHistoryModel(string customerId);
        Task<RewardPointsHistory> InsertRewardPointsHistory(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage);
        Task<IEnumerable<AddressModel>> PrepareAddressModel(Customer customer);
        Task DeleteAddress(Customer customer, Address address);
        Task PrepareAddressModel(CustomerAddressModel model, Address address, Customer customer, bool excludeProperties);
        Task<Address> InsertAddressModel(Customer customer, CustomerAddressModel model, string customAttributes);
        Task<Address> UpdateAddressModel(Customer customer, Address address, CustomerAddressModel model, string customAttributes);
        Task<(IEnumerable<CustomerModel.OrderModel> orderModels, int totalCount)> PrepareOrderModel(string customerId, int pageIndex, int pageSize);
        CustomerReportsModel PrepareCustomerReportsModel();
        Task<IList<RegisteredCustomerReportLineModel>> GetReportRegisteredCustomersModel(string storeId);
        Task<(IEnumerable<BestCustomerReportLineModel> bestCustomerReportLineModels, int totalCount)> PrepareBestCustomerReportLineModel(BestCustomersReportModel model, int orderBy, int pageIndex, int pageSize);
        Task<IList<ShoppingCartItemModel>> PrepareShoppingCartItemModel(string customerId, int cartTypeId);
        Task DeleteCart(Customer customer, string id);
        Task<(IEnumerable<CustomerModel.ProductPriceModel> productPriceModels, int totalCount)> PrepareProductPriceModel(string customerId, int pageIndex, int pageSize);
        Task<(IEnumerable<CustomerModel.ProductModel> productModels, int totalCount)> PreparePersonalizedProducts(string customerId, int pageIndex, int pageSize);
        Task<CustomerModel.AddProductModel> PrepareCustomerModelAddProductModel();
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerModel.AddProductModel model, int pageIndex, int pageSize);
        Task InsertCustomerAddProductModel(string customerId, bool personalized, CustomerModel.AddProductModel model);
        Task UpdateProductPrice(CustomerModel.ProductPriceModel model);
        Task DeleteProductPrice(string id);
        Task UpdatePersonalizedProduct(CustomerModel.ProductModel model);
        Task DeletePersonalizedProduct(string id);
        Task<(IEnumerable<CustomerModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string customerId, int pageIndex, int pageSize);
        Task<(IEnumerable<ContactFormModel> contactFormModels, int totalCount)> PrepareContactFormModel(string customerId, string vendorId, int pageIndex, int pageSize);
        Task<(IEnumerable<CustomerModel.BackInStockSubscriptionModel> backInStockSubscriptionModels, int totalCount)> PrepareBackInStockSubscriptionModel(string customerId, int pageIndex, int pageSize);
        Task<IList<CustomerModel.CustomerNote>> PrepareCustomerNoteList(string customerId);
        Task<CustomerNote> InsertCustomerNote(string customerId, string downloadId, bool displayToCustomer, string title, string message);
        Task DeleteCustomerNote(string id, string customerId);
    }
}
