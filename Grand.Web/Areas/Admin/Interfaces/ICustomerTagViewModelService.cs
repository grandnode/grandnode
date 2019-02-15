using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerTagViewModelService
    {
        CustomerModel PrepareCustomerModelForList(Customer customer);
        CustomerTagModel PrepareCustomerTagModel();
        CustomerTag InsertCustomerTagModel(CustomerTagModel model);
        CustomerTag UpdateCustomerTagModel(CustomerTag customerTag, CustomerTagModel model);
        void DeleteCustomerTag(CustomerTag customerTag);
        CustomerTagProductModel.AddProductModel PrepareProductModel(string customerTagId);
        (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerTagProductModel.AddProductModel model, int pageIndex, int pageSize);
        void InsertProductModel(CustomerTagProductModel.AddProductModel model);
    }
}
