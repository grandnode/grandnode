using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerRoleViewModelService
    {
        CustomerRoleModel PrepareCustomerRoleModel(CustomerRole customerRole);
        CustomerRoleModel PrepareCustomerRoleModel();
        CustomerRole InsertCustomerRoleModel(CustomerRoleModel model);
        CustomerRole UpdateCustomerRoleModel(CustomerRole customerRole, CustomerRoleModel model);
        void DeleteCustomerRole(CustomerRole customerRole);
        CustomerRoleModel.AssociateProductToCustomerRoleModel PrepareAssociateProductToCustomerRoleModel();
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerRoleModel.AssociateProductToCustomerRoleModel model, int pageIndex, int pageSize);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerRoleProductModel.AddProductModel model, int pageIndex, int pageSize);
        IList<CustomerRoleProductModel> PrepareCustomerRoleProductModel(string customerRoleId);
        CustomerRoleProductModel.AddProductModel PrepareProductModel(string customerRoleId);
        void InsertProductModel(CustomerRoleProductModel.AddProductModel model);
    }
}
