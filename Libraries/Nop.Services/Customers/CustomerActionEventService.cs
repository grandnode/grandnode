using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;
using Nop.Services.Localization;
using System.Web;
using Nop.Core.Domain.Logging;

namespace Nop.Services.Customers
{
    public partial class CustomerActionEventService : ICustomerActionEventService
    {
        #region Fields

        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
        private readonly IRepository<CustomerActionConditionType> _customerActionConditionTypeRepository;
        private readonly IRepository<Banner> _bannerRepository;
        private readonly IRepository<BannerActive> _bannerActiveRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICustomerTagService _customerTagService;
        private readonly HttpContextBase _httpContext;
        #endregion

        #region Ctor

        public CustomerActionEventService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            IRepository<CustomerActionConditionType> customerActionConditionTypeRepository,
            IRepository<Banner> bannerRepository,
            IRepository<BannerActive> bannerActiveRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IEventPublisher eventPublisher,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IMessageTemplateService messageTemplateService,
            IWorkflowMessageService workflowMessageService,
            IWorkContext workContext,
            ICustomerService customerService,
            ICustomerTagService customerTagService,
            HttpContextBase httpContext)
        {
            this._customerActionRepository = customerActionRepository;
            this._customerActionTypeRepository = customerActionTypeRepository;
            this._customerActionConditionTypeRepository = customerActionConditionTypeRepository;
            this._customerActionHistoryRepository = customerActionHistoryRepository;
            this._bannerRepository = bannerRepository;
            this._bannerActiveRepository = bannerActiveRepository;
            this._activityLogRepository = activityLogRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._eventPublisher = eventPublisher;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._messageTemplateService = messageTemplateService;
            this._workflowMessageService = workflowMessageService;
            this._workContext = workContext;
            this._customerService = customerService;
            this._customerTagService = customerTagService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities

        #region Action
        protected bool UsedAction(int actionId, int customerId)
        {
            var query = from u in _customerActionHistoryRepository.Table
                        where u.CustomerId == customerId && u.CustomerActionId == actionId
                        select u.Id;
            if (query.Count() > 0)
                return true;
            
            return false;
        }

        protected void SaveActionToCustomer(int actionId, int customerId)
        {
            _customerActionHistoryRepository.Insert(new CustomerActionHistory() { CustomerId = customerId, CustomerActionId = actionId });
        }
        #endregion

        #region Condition
        protected bool Condition(CustomerAction action, Product product, string attributesXml, int customerId, string currentUrl, string previousUrl)
        {
            bool cond = false;
            foreach (var item in action.Conditions)
            {
                #region product
                if (product != null)
                {

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
                if(action.ActionTypeId == (int)CustomerActionTypeEnum.Viewed)
                {
                    cond = false;
                    if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Category)
                    {
                        var _actLogType = (from a in _activityLogTypeRepository.Table
                                  where a.SystemKeyword == "PublicStore.ViewCategory"
                                           select a).FirstOrDefault();
                        if(_actLogType != null)
                        {
                            if(_actLogType.Enabled)
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

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerRole)
                {
                    cond = ConditionCustomerRole(item, customerId);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerTag)
                {
                    cond = ConditionCustomerTag(item, customerId);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomerRegisterField)
                {
                    cond = ConditionCustomerRegister(item, customerId);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.UrlCurrent)
                {
                    cond = item.UrlCurrent.Select(x=>x.Name).Contains(currentUrl);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.UrlReferrer)
                {
                    cond = item.UrlReferrer.Select(x => x.Name).Contains(previousUrl);
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
        protected bool ConditionCategory(CustomerAction.ActionCondition condition, ICollection<int> categorties)
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

        protected bool ConditionManufacturer(CustomerAction.ActionCondition condition, ICollection<int> manufacturers)
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

        protected bool ConditionProducts(CustomerAction.ActionCondition condition, int productId)
        {
            return condition.Products.Contains(productId);
        }

        protected bool ConditionProducts(CustomerAction.ActionCondition condition, ICollection<int> products)
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
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                var attributes = _productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                foreach (var attr in attributes)
                {
                    var attributeValuesStr = _productAttributeParser.ParseValues(AttributesXml, attr.Id);
                    foreach (var attrV in attributeValuesStr)
                    {
                        var attrsv = attr.ProductAttributeValues.Where(x => x.Id == Convert.ToInt32(attrV)).FirstOrDefault();
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
                    var attributes = _productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                    if (attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId).Count() > 0)
                    {
                        cond = false;
                        foreach (var attr in attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId))
                        {
                            var attributeValuesStr = _productAttributeParser.ParseValues(AttributesXml, attr.Id);
                            foreach (var attrV in attributeValuesStr)
                            {
                                var attrsv = attr.ProductAttributeValues.Where(x => x.Id == Convert.ToInt32(attrV)).FirstOrDefault();
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

        protected bool ConditionVendors(CustomerAction.ActionCondition condition, int vendorId)
        {
            return condition.Vendors.Contains(vendorId);
        }

        protected bool ConditionCustomerRole(CustomerAction.ActionCondition condition, int customerId)
        {
            bool cond = false;
            var customer = _customerService.GetCustomerById(customerId);
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

        protected bool ConditionCustomerTag(CustomerAction.ActionCondition condition, int customerId)
        {
            bool cond = false;
            var customer = _customerService.GetCustomerById(customerId);
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

        protected bool ConditionCustomerRegister(CustomerAction.ActionCondition condition, int customerId)
        {
            bool cond = false;
            var customer = _customerService.GetCustomerById(customerId);
            if (customer != null)
            {
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    cond = true;
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (customer.GenericAttributes.Where(x => x.Key == item.RegisterField && x.Value == item.RegisterValue).Count() == 0)
                            cond = false;
                    }
                }
                if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
                {
                    foreach (var item in condition.CustomerRegistration)
                    {
                        if (customer.GenericAttributes.Where(x => x.Key == item.RegisterField && x.Value == item.RegisterValue).Count() > 0)
                            cond = true;
                    }
                }
            }
            return cond;
        }
        #endregion

        #region Reaction
        protected void Reaction(CustomerAction action, int customerId, ShoppingCartItem cartItem, Order order)
        {
            if (action.ReactionType == CustomerReactionTypeEnum.Banner)
            {
                var banner = _bannerRepository.GetById(action.BannerId);
                PrepareBanner(action, banner, customerId);
            }

            if (action.ReactionType == CustomerReactionTypeEnum.Email)
            {
                if (action.ActionTypeId == (int)CustomerActionTypeEnum.AddToCart)
                {
                    if(cartItem!=null)
                        _workflowMessageService.SendCustomerActionEvent_AddToCart_Notification(action, cartItem,
                            _workContext.WorkingLanguage.Id, customerId);
                }

                if (action.ActionTypeId == (int)CustomerActionTypeEnum.AddOrder)
                {
                    if(order!=null)
                        _workflowMessageService.SendCustomerActionEvent_AddToOrder_Notification(action, order,
                            _workContext.WorkingLanguage.Id);
                }

                if (action.ActionTypeId != (int)CustomerActionTypeEnum.AddOrder && action.ActionTypeId != (int)CustomerActionTypeEnum.AddToCart)
                {
                    _workflowMessageService.SendCustomerActionEvent_Notification(action, 
                        _workContext.WorkingLanguage.Id, customerId);
                }
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerRole)
            {
                AssignToCustomerRole(action, customerId);
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerTag)
            {
                AssignToCustomerTag(action, customerId);
            }

            SaveActionToCustomer(action.Id, customerId);

        }
        protected void PrepareBanner(CustomerAction action, Banner banner, int customerId)
        {
            var banneractive = new BannerActive()
            {
                Body = banner.Body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = banner.Name
            };
            _bannerActiveRepository.Insert(banneractive);
        }

        protected void AssignToCustomerRole(CustomerAction action, int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if(customer.CustomerRoles.Where(x=>x.Id == action.CustomerRoleId).Count() == 0)
            {
                var customerRole = _customerService.GetCustomerRoleById(action.CustomerRoleId);
                if(customerRole!=null)
                {
                    customerRole.CustomerId = customer.Id;
                    _customerService.InsertCustomerRoleInCustomer(customerRole);
                }
            }
        }

        protected void AssignToCustomerTag(CustomerAction action, int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            if (customer.CustomerTags.Where(x => x == action.CustomerTagId).Count() == 0)
            {
                _customerTagService.InsertTagToCustomer(action.CustomerTagId, customerId);
            }
        }

        #endregion

        #endregion

        #region Methods

        public virtual void AddToCart(ShoppingCartItem cart, Product product)
        {
            var actionType = _customerActionTypeRepository.Table.Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddToCart.ToString()).FirstOrDefault();
            if (actionType.Enabled)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc  && datetimeUtcNow <= a.EndDateTimeUtc 
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, cart.CustomerId))
                    {
                        if (Condition(item, product, cart.AttributesXml, cart.CustomerId, null, null))
                        {
                            Reaction(item, cart.CustomerId, cart, null);
                        }
                    }
                }
            }
        }

        public virtual void AddOrder(Order order)
        {
            var actionType = _customerActionTypeRepository.Table.Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddOrder.ToString()).FirstOrDefault();
            if (actionType.Enabled)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, order.CustomerId))
                    {
                        foreach(var orderItem in order.OrderItems)
                        {
                            if (Condition(item, orderItem.Product, orderItem.AttributesXml, order.CustomerId, null, null))
                            {
                                Reaction(item, order.CustomerId, null, order);
                                break;
                            }
                        }
                    }
                }

            }
        }

        public virtual void Url(int customerId, string currentUrl, string previousUrl)
        {
            var actionType = _customerActionTypeRepository.Table.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Url.ToString()).FirstOrDefault();
            if (actionType.Enabled)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, customerId))
                    {
                        if (Condition(item, null, null, customerId, currentUrl, previousUrl))
                        {
                            Reaction(item, customerId, null, null);
                        }
                    }
                }

            }
        }

        public virtual void Viewed(int customerId, string currentUrl, string previousUrl)
        {
            var actionType = _customerActionTypeRepository.Table.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Viewed.ToString()).FirstOrDefault();
            if (actionType.Enabled)
            {
                var datetimeUtcNow = DateTime.UtcNow;
                var query = from a in _customerActionRepository.Table
                            where a.Active == true && a.ActionTypeId == actionType.Id
                                    && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                            select a;

                foreach (var item in query.ToList())
                {
                    if (!UsedAction(item.Id, customerId))
                    {
                        if (Condition(item, null, null, customerId, currentUrl, previousUrl))
                        {
                            Reaction(item, customerId, null, null);
                        }
                    }
                }

            }
        }

        #endregion
    }
}
