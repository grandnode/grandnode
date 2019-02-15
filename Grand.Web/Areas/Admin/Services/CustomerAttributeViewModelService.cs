using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public virtual CustomerAttribute InsertCustomerAttributeModel(CustomerAttributeModel model)
        {
            var customerAttribute = model.ToEntity();
            _customerAttributeService.InsertCustomerAttribute(customerAttribute);
            return customerAttribute;
        }

        public virtual CustomerAttributeValue InsertCustomerAttributeValueModel(CustomerAttributeValueModel model)
        {
            var cav = model.ToEntity();
            _customerAttributeService.InsertCustomerAttributeValue(cav);
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

        public virtual IEnumerable<CustomerAttributeModel> PrepareCustomerAttributes()
        {
            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
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

        public virtual IEnumerable<CustomerAttributeValueModel> PrepareCustomerAttributeValues(string customerAttributeId)
        {
            var values = _customerAttributeService.GetCustomerAttributeById(customerAttributeId).CustomerAttributeValues;
            return values.Select(x => new CustomerAttributeValueModel
            {
                Id = x.Id,
                CustomerAttributeId = x.CustomerAttributeId,
                Name = x.Name,
                IsPreSelected = x.IsPreSelected,
                DisplayOrder = x.DisplayOrder,
            });
        }

        public virtual CustomerAttribute UpdateCustomerAttributeModel(CustomerAttributeModel model, CustomerAttribute customerAttribute)
        {
            customerAttribute = model.ToEntity(customerAttribute);
            _customerAttributeService.UpdateCustomerAttribute(customerAttribute);
            return customerAttribute;
        }

        public virtual CustomerAttributeValue UpdateCustomerAttributeValueModel(CustomerAttributeValueModel model, CustomerAttributeValue customerAttributeValue)
        {
            customerAttributeValue = model.ToEntity(customerAttributeValue);
            _customerAttributeService.UpdateCustomerAttributeValue(customerAttributeValue);
            return customerAttributeValue;
        }

        public virtual void DeleteCustomerAttribute(string id)
        {
            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(id);
            _customerAttributeService.DeleteCustomerAttribute(customerAttribute);
        }

        public virtual void DeleteCustomerAttributeValue(CustomerAttributeValueModel model)
        {
            var av = _customerAttributeService.GetCustomerAttributeById(model.CustomerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                throw new ArgumentException("No customer attribute value found with the specified id");
            _customerAttributeService.DeleteCustomerAttributeValue(cav);
        }
    }
}
