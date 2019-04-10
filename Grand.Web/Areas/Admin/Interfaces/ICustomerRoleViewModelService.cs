using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerRoleViewModelService
    {
        Task<CustomerRoleModel> PrepareCustomerRoleModel(CustomerRole customerRole);
        CustomerRoleModel PrepareCustomerRoleModel();
        Task<CustomerRole> InsertCustomerRoleModel(CustomerRoleModel model);
        Task<CustomerRole> UpdateCustomerRoleModel(CustomerRole customerRole, CustomerRoleModel model);
        Task DeleteCustomerRole(CustomerRole customerRole);
        Task<CustomerRoleModel.AssociateProductToCustomerRoleModel> PrepareAssociateProductToCustomerRoleModel();
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerRoleModel.AssociateProductToCustomerRoleModel model, int pageIndex, int pageSize);
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerRoleProductModel.AddProductModel model, int pageIndex, int pageSize);
        Task<IList<CustomerRoleProductModel>> PrepareCustomerRoleProductModel(string customerRoleId);
        Task<CustomerRoleProductModel.AddProductModel> PrepareProductModel(string customerRoleId);
        Task InsertProductModel(CustomerRoleProductModel.AddProductModel model);
    }
}
