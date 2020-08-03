using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Logging;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Customers;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core;

namespace Grand.Services.Commands.Handlers.Customers
{
    public class CustomerActionEventConditionCommandHandler : IRequestHandler<CustomerActionEventConditionCommand, bool>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;

        public CustomerActionEventConditionCommandHandler(
            IServiceProvider serviceProvider,
            IProductService productService,
            ICustomerService customerService,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository)
        {
            _serviceProvider = serviceProvider;
            _productService = productService;
            _customerService = customerService;
            _activityLogRepository = activityLogRepository;
            _activityLogTypeRepository = activityLogTypeRepository;
        }

        public async Task<bool> Handle(CustomerActionEventConditionCommand request, CancellationToken cancellationToken)
        {
            return await Condition(
                request.CustomerActionTypes,
                request.Action,
                request.ProductId,
                request.AttributesXml,
                request.CustomerId,
                request.CurrentUrl,
                request.PreviousUrl);
        }

        protected async Task<bool> Condition(IList<CustomerActionType> customerActionTypes, CustomerAction action, string productId, string attributesXml, string customerId, string currentUrl, string previousUrl)
        {
            if (!action.Conditions.Any())
                return true;

            bool cond = false;
            foreach (var item in action.Conditions)
            {
                #region product
                if (!string.IsNullOrEmpty(productId))
                {
                    var product = await _productService.GetProductById(productId);

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Category)
                    {
                        cond = ConditionCategory(item, product.ProductCategories);
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Manufacturer)
                    {
                        cond = ConditionManufacturer(item, product.ProductManufacturers);
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Product)
                    {
                        cond = ConditionProducts(item, product.Id);
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.ProductAttribute)
                    {
                        if (!String.IsNullOrEmpty(attributesXml))
                        {
                            cond = ConditionProductAttribute(item, product, attributesXml);
                        }
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.ProductSpecification)
                    {
                        cond = ConditionSpecificationAttribute(item, product.ProductSpecificationAttributes);
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Vendor)
                    {
                        cond = ConditionVendors(item, product.VendorId);
                    }

                }
                #endregion

                #region Action type viewed
                if (action.ActionTypeId == customerActionTypes.FirstOrDefault(x => x.SystemKeyword == "Viewed").Id)
                {
                    cond = false;
                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Category)
                    {
                        var _actLogType = (from a in _activityLogTypeRepository.Table
                                           where a.SystemKeyword == "PublicStore.ViewCategory"
                                           select a).FirstOrDefault();
                        if (_actLogType != null)
                        {
                            if (_actLogType.Enabled)
                            {
                                var productCategory = (from p in _activityLogRepository.Table
                                                       where p.CustomerId == customerId && p.ActivityLogTypeId == _actLogType.Id
                                                       select p.EntityKeyId).Distinct().ToList();
                                cond = ConditionCategory(item, productCategory);
                            }
                        }
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Manufacturer)
                    {
                        cond = false;
                        var _actLogType = (from a in _activityLogTypeRepository.Table
                                           where a.SystemKeyword == "PublicStore.ViewManufacturer"
                                           select a).FirstOrDefault();
                        if (_actLogType != null)
                        {
                            if (_actLogType.Enabled)
                            {
                                var productManufacturer = (from p in _activityLogRepository.Table
                                                           where p.CustomerId == customerId && p.ActivityLogTypeId == _actLogType.Id
                                                           select p.EntityKeyId).Distinct().ToList();
                                cond = ConditionManufacturer(item, productManufacturer);
                            }
                        }
                    }

                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Product)
                    {
                        cond = false;
                        var _actLogType = (from a in _activityLogTypeRepository.Table
                                           where a.SystemKeyword == "PublicStore.ViewProduct"
                                           select a).FirstOrDefault();
                        if (_actLogType != null)
                        {
                            if (_actLogType.Enabled)
                            {
                                var products = (from p in _activityLogRepository.Table
                                                where p.CustomerId == customerId && p.ActivityLogTypeId == _actLogType.Id
                                                select p.EntityKeyId).Distinct().ToList();
                                cond = ConditionProducts(item, products);
                            }
                        }
                    }
                }
                #endregion

                var customer = await _customerService.GetCustomerById(customerId);

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerRole)
                {
                    cond = ConditionCustomerRole(item, customer);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerTag)
                {
                    cond = ConditionCustomerTag(item, customer);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerRegisterField)
                {
                    cond = await ConditionCustomerRegister(item, customer);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomCustomerAttribute)
                {
                    cond = await ConditionCustomerAttribute(item, customer);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.UrlCurrent)
                {
                    cond = item.UrlCurrent.Select(x => x.Name).Contains(currentUrl);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.UrlReferrer)
                {
                    cond = item.UrlReferrer.Select(x => x.Name).Contains(previousUrl);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Store)
                {
                    var storeContext = _serviceProvider.GetRequiredService<IStoreContext>();
                    cond = ConditionStores(item, storeContext.CurrentStore.Id);
                }

                if (action.Condition == CustomerActionConditionEnum.OneOfThem && cond)
                    return true;
                if (action.Condition == CustomerActionConditionEnum.AllOfThem && !cond)
                    return false;
            }

            return cond;
        }
        protected bool ConditionCategory(CustomerAction.ActionCondition condition, ICollection<ProductCategory> categorties)
        {
            bool cond = true;
            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = categorties.Select(x => x.CategoryId).ContainsAll(condition.Categories);
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                cond = categorties.Select(x => x.CategoryId).ContainsAny(condition.Categories);
            }

            return cond;
        }
        protected bool ConditionCategory(CustomerAction.ActionCondition condition, ICollection<string> categorties)
        {
            bool cond = true;
            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = categorties.ContainsAll(condition.Categories);
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                cond = categorties.ContainsAny(condition.Categories);
            }

            return cond;
        }
        protected bool ConditionManufacturer(CustomerAction.ActionCondition condition, ICollection<ProductManufacturer> manufacturers)
        {
            bool cond = true;

            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = manufacturers.Select(x => x.ManufacturerId).ContainsAll(condition.Manufacturers);
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                cond = manufacturers.Select(x => x.ManufacturerId).ContainsAny(condition.Manufacturers);
            }

            return cond;
        }
        protected bool ConditionManufacturer(CustomerAction.ActionCondition condition, ICollection<string> manufacturers)
        {
            bool cond = true;

            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = manufacturers.ContainsAll(condition.Manufacturers);
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                cond = manufacturers.ContainsAny(condition.Manufacturers);
            }

            return cond;
        }
        protected bool ConditionProducts(CustomerAction.ActionCondition condition, string productId)
        {
            return condition.Products.Contains(productId);
        }
        protected bool ConditionStores(CustomerAction.ActionCondition condition, string storeId)
        {
            return condition.Stores.Contains(storeId);
        }
        protected bool ConditionProducts(CustomerAction.ActionCondition condition, ICollection<string> products)
        {
            bool cond = true;
            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = products.ContainsAll(condition.Products);
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                cond = products.ContainsAny(condition.Products);
            }

            return cond;
        }
        protected bool ConditionProductAttribute(CustomerAction.ActionCondition condition, Product product, string AttributesXml)
        {
            bool cond = false;
            var productAttributeParser = _serviceProvider.GetRequiredService<IProductAttributeParser>();
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                var attributes = productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                foreach (var attr in attributes)
                {
                    var attributeValuesStr = productAttributeParser.ParseValues(AttributesXml, attr.Id);
                    foreach (var attrV in attributeValuesStr)
                    {
                        var attrsv = attr.ProductAttributeValues.Where(x => x.Id == attrV).FirstOrDefault();
                        if (attrsv != null)
                            if (condition.ProductAttribute.Where(x => x.ProductAttributeId == attr.ProductAttributeId && x.Name == attrsv.Name).Count() > 0)
                            {
                                cond = true;
                            }
                    }
                }
            }
            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var itemPA in condition.ProductAttribute)
                {
                    var attributes = productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                    if (attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId).Count() > 0)
                    {
                        cond = false;
                        foreach (var attr in attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId))
                        {
                            var attributeValuesStr = productAttributeParser.ParseValues(AttributesXml, attr.Id);
                            foreach (var attrV in attributeValuesStr)
                            {
                                var attrsv = attr.ProductAttributeValues.Where(x => x.Id == attrV).FirstOrDefault();
                                if (attrsv != null)
                                {
                                    if (attrsv.Name == itemPA.Name)
                                    {
                                        cond = true;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        if (!cond)
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return cond;
        }
        protected bool ConditionSpecificationAttribute(CustomerAction.ActionCondition condition, ICollection<ProductSpecificationAttribute> productspecificationattribute)
        {
            bool cond = false;

            if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
            {
                cond = true;
                foreach (var spec in condition.ProductSpecifications)
                {
                    if (productspecificationattribute.Where(x => x.SpecificationAttributeId == spec.ProductSpecyficationId && x.SpecificationAttributeOptionId == spec.ProductSpecyficationValueId).Count() == 0)
                        cond = false;
                }
            }
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                foreach (var spec in productspecificationattribute)
                {
                    if (condition.ProductSpecifications.Where(x => x.ProductSpecyficationId == spec.SpecificationAttributeId && x.ProductSpecyficationValueId == spec.SpecificationAttributeOptionId).Count() > 0)
                        cond = true;
                }
            }

            return cond;
        }
        protected bool ConditionVendors(CustomerAction.ActionCondition condition, string vendorId)
        {
            return condition.Vendors.Contains(vendorId);
        }
        protected bool ConditionCustomerRole(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var customerRoles = customer.CustomerRoles;
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    cond = customerRoles.Select(x => x.Id).ContainsAll(condition.CustomerRoles);
                }
                if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
                {
                    cond = customerRoles.Select(x => x.Id).ContainsAny(condition.CustomerRoles);
                }
            }
            return cond;
        }
        protected bool ConditionCustomerTag(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var customerTags = customer.CustomerTags;
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    cond = customerTags.Select(x => x).ContainsAll(condition.CustomerTags);
                }
                if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
                {
                    cond = customerTags.Select(x => x).ContainsAny(condition.CustomerTags);
                }
            }
            return cond;
        }
        protected async Task<bool> ConditionCustomerRegister(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var _genericAttributes = customer.GenericAttributes;
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    cond = true;
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (_genericAttributes.Where(x => x.Key == item.RegisterField && x.Value.ToLower() == item.RegisterValue.ToLower()).Count() == 0)
                            cond = false;
                    }
                }
                if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
                {
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (_genericAttributes.Where(x => x.Key == item.RegisterField && x.Value.ToLower() == item.RegisterValue.ToLower()).Count() > 0)
                            cond = true;
                    }
                }
            }
            return await Task.FromResult(cond);
        }
        protected async Task<bool> ConditionCustomerAttribute(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var customerAttributeParser = _serviceProvider.GetRequiredService<ICustomerAttributeParser>();

                var _genericAttributes = customer.GenericAttributes;
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    var customCustomerAttributes = _genericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes");
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = await customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
                            cond = true;
                            foreach (var item in condition.CustomCustomerAttributes)
                            {
                                var _fields = item.RegisterField.Split(':');
                                if (_fields.Count() > 1)
                                {
                                    if (selectedValues.Where(x => x.CustomerAttributeId == _fields.FirstOrDefault() && x.Id == _fields.LastOrDefault()).Count() == 0)
                                        cond = false;
                                }
                                else
                                    cond = false;
                            }
                        }
                    }
                }
                if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
                {
                    var customCustomerAttributes = _genericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes");
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = await customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
                            foreach (var item in condition.CustomCustomerAttributes)
                            {
                                var _fields = item.RegisterField.Split(':');
                                if (_fields.Count() > 1)
                                {
                                    if (selectedValues.Where(x => x.CustomerAttributeId == _fields.FirstOrDefault() && x.Id == _fields.LastOrDefault()).Count() > 0)
                                        cond = true;
                                }
                            }
                        }
                    }
                }
            }
            return cond;
        }

    }
}
