using Grand.Core.Domain.Customers;
using Grand.Web.Areas.Admin.Models.Customers;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICustomerAttributeViewModelService
    {
        IEnumerable<CustomerAttributeModel> PrepareCustomerAttributes();
        CustomerAttributeModel PrepareCustomerAttributeModel();
        CustomerAttributeModel PrepareCustomerAttributeModel(CustomerAttribute customerAttribute);
        CustomerAttribute InsertCustomerAttributeModel(CustomerAttributeModel model);
        CustomerAttribute UpdateCustomerAttributeModel(CustomerAttributeModel model, CustomerAttribute customerAttribute);
        void DeleteCustomerAttribute(string id);
        void DeleteCustomerAttributeValue(CustomerAttributeValueModel model);
        IEnumerable<CustomerAttributeValueModel> PrepareCustomerAttributeValues(string customerAttributeId);
        CustomerAttributeValueModel PrepareCustomerAttributeValueModel(string customerAttributeId);
        CustomerAttributeValue InsertCustomerAttributeValueModel(CustomerAttributeValueModel model);
        CustomerAttributeValueModel PrepareCustomerAttributeValueModel(CustomerAttributeValue customerAttributeValue);
        CustomerAttributeValue UpdateCustomerAttributeValueModel(CustomerAttributeValueModel model, CustomerAttributeValue customerAttributeValue);
    }
}
