using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Models.ShoppingCart;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerViewModelService
    {
        CustomerListModel PrepareCustomerListModel();
        (IEnumerable<CustomerModel> customerModelList, int totalCount) PrepareCustomerList(CustomerListModel model,
            string[] searchCustomerRoleIds, string[] searchCustomerTagIds, int pageIndex, int pageSize);
        void PrepareCustomerModel(CustomerModel model, Customer customer, bool excludeProperties);
        string ValidateCustomerRoles(IList<CustomerRole> customerRoles);
        Customer InsertCustomerModel(CustomerModel model);
        Customer UpdateCustomerModel(Customer customer, CustomerModel model);
        void DeleteCustomer(Customer customer);
        void SendEmail(Customer customer, CustomerModel.SendEmailModel model);
        void SendPM(Customer customer, CustomerModel.SendPmModel model);
        IEnumerable<CustomerModel.RewardPointsHistoryModel> PrepareRewardPointsHistoryModel(string customerId);
        RewardPointsHistory InsertRewardPointsHistory(string customerId, string storeId, int addRewardPointsValue, string addRewardPointsMessage);
        IEnumerable<AddressModel> PrepareAddressModel(Customer customer);
        void DeleteAddress(Customer customer, Address address);
        void PrepareAddressModel(CustomerAddressModel model, Address address, Customer customer, bool excludeProperties);
        Address InsertAddressModel(Customer customer, CustomerAddressModel model, string customAttributes);
        Address UpdateAddressModel(Customer customer, Address address, CustomerAddressModel model, string customAttributes);
        (IEnumerable<CustomerModel.OrderModel> orderModels, int totalCount) PrepareOrderModel(string customerId, int pageIndex, int pageSize);
        CustomerReportsModel PrepareCustomerReportsModel();
        IList<RegisteredCustomerReportLineModel> GetReportRegisteredCustomersModel();
        (IEnumerable<BestCustomerReportLineModel> bestCustomerReportLineModels, int totalCount) PrepareBestCustomerReportLineModel(BestCustomersReportModel model, int orderBy, int pageIndex, int pageSize);
        IList<ShoppingCartItemModel> PrepareShoppingCartItemModel(string customerId, int cartTypeId);
        void DeleteCart(Customer customer, string id);
        (IEnumerable<CustomerModel.ProductPriceModel> productPriceModels, int totalCount) PrepareProductPriceModel(string customerId, int pageIndex, int pageSize);
        (IEnumerable<CustomerModel.ProductModel> productModels, int totalCount) PreparePersonalizedProducts(string customerId, int pageIndex, int pageSize);
        CustomerModel.AddProductModel PrepareCustomerModelAddProductModel();
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerModel.AddProductModel model, int pageIndex, int pageSize);
        void InsertCustomerAddProductModel(string customerId, bool personalized, CustomerModel.AddProductModel model);
        void UpdateProductPrice(CustomerModel.ProductPriceModel model);
        void DeleteProductPrice(string id);
        void UpdatePersonalizedProduct(CustomerModel.ProductModel model);
        void DeletePersonalizedProduct(string id);
        (IEnumerable<CustomerModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string customerId, int pageIndex, int pageSize);
        (IEnumerable<ContactFormModel> contactFormModels, int totalCount) PrepareContactFormModel(string customerId, string vendorId, int pageIndex, int pageSize);
        (IEnumerable<CustomerModel.BackInStockSubscriptionModel> backInStockSubscriptionModels, int totalCount) PrepareBackInStockSubscriptionModel(string customerId, int pageIndex, int pageSize);
        IList<CustomerModel.CustomerNote> PrepareCustomerNoteList(string customerId);
        CustomerNote InsertCustomerNote(string customerId, string downloadId, bool displayToCustomer, string title, string message);
        void DeleteCustomerNote(string id, string customerId);
    }
}
