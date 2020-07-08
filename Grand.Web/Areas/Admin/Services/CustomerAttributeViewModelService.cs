using Grand.Core;
using Grand.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CustomerAttributeViewModelService : ICustomerAttributeViewModelService
    {
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        public CustomerAttributeViewModelService(ICustomerAttributeService customerAttributeService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _customerAttributeService = customerAttributeService;
            _localizationService = localizationService;
            _workContext = workContext;
        }

        public virtual async Task<CustomerAttribute> InsertCustomerAttributeModel(CustomerAttributeModel model)
        {
            var customerAttribute = model.ToEntity();
            await _customerAttributeService.InsertCustomerAttribute(customerAttribute);
            return customerAttribute;
        }

        public virtual async Task<CustomerAttributeValue> InsertCustomerAttributeValueModel(CustomerAttributeValueModel model)
        {
            var cav = model.ToEntity();
            await _customerAttributeService.InsertCustomerAttributeValue(cav);
            return cav;
        }

        public virtual CustomerAttributeModel PrepareCustomerAttributeModel()
        {
            var model = new CustomerAttributeModel();
            return model;
        }

        public virtual CustomerAttributeModel PrepareCustomerAttributeModel(CustomerAttribute customerAttribute)
        {
            var model = customerAttribute.ToModel();
            return model;
        }

        public virtual async Task<IEnumerable<CustomerAttributeModel>> PrepareCustomerAttributes()
        {
            var customerAttributes = await _customerAttributeService.GetAllCustomerAttributes();
            return customerAttributes.Select(x =>
            {
                var attributeModel = x.ToModel();
                attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                return attributeModel;
            });
        }

        public virtual CustomerAttributeValueModel PrepareCustomerAttributeValueModel(string customerAttributeId)
        {
            var model = new CustomerAttributeValueModel();
            model.CustomerAttributeId = customerAttributeId;
            return model;
        }

        public virtual CustomerAttributeValueModel PrepareCustomerAttributeValueModel(CustomerAttributeValue customerAttributeValue)
        {
            var model = customerAttributeValue.ToModel();
            return model;
        }

        public virtual async Task<IEnumerable<CustomerAttributeValueModel>> PrepareCustomerAttributeValues(string customerAttributeId)
        {
            var values = (await _customerAttributeService.GetCustomerAttributeById(customerAttributeId)).CustomerAttributeValues;
            return values.Select(x => new CustomerAttributeValueModel
            {
                Id = x.Id,
                CustomerAttributeId = x.CustomerAttributeId,
                Name = x.Name,
                IsPreSelected = x.IsPreSelected,
                DisplayOrder = x.DisplayOrder,
            });
        }

        public virtual async Task<CustomerAttribute> UpdateCustomerAttributeModel(CustomerAttributeModel model, CustomerAttribute customerAttribute)
        {
            customerAttribute = model.ToEntity(customerAttribute);
            await _customerAttributeService.UpdateCustomerAttribute(customerAttribute);
            return customerAttribute;
        }

        public virtual async Task<CustomerAttributeValue> UpdateCustomerAttributeValueModel(CustomerAttributeValueModel model, CustomerAttributeValue customerAttributeValue)
        {
            customerAttributeValue = model.ToEntity(customerAttributeValue);
            await _customerAttributeService.UpdateCustomerAttributeValue(customerAttributeValue);
            return customerAttributeValue;
        }

        public virtual async Task DeleteCustomerAttribute(string id)
        {
            var customerAttribute = await _customerAttributeService.GetCustomerAttributeById(id);
            await _customerAttributeService.DeleteCustomerAttribute(customerAttribute);
        }

        public virtual async Task DeleteCustomerAttributeValue(CustomerAttributeValueModel model)
        {
            var av = await _customerAttributeService.GetCustomerAttributeById(model.CustomerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                throw new ArgumentException("No customer attribute value found with the specified id");
            await _customerAttributeService.DeleteCustomerAttributeValue(cav);
        }
    }
}
