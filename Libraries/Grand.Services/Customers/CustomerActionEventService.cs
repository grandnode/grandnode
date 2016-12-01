using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Core.Domain.Catalog;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Messages;
using Grand.Services.Localization;
using System.Web;
using Grand.Core.Domain.Logging;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Services.Helpers;

namespace Grand.Services.Customers
{
    public partial class CustomerActionEventService : ICustomerActionEventService
    {
        #region Fields
        private const string CUSTOMER_ACTION_TYPE = "Nop.customer.action.type";

        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
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
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerTagService _customerTagService;
        private readonly HttpContextBase _httpContext;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public CustomerActionEventService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
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
            ICustomerAttributeService customerAttributeService,
            ICustomerAttributeParser customerAttributeParser,
            ICustomerTagService customerTagService,
            HttpContextBase httpContext,
            ICacheManager cacheManager)
        {
            this._customerActionRepository = customerActionRepository;
            this._customerActionTypeRepository = customerActionTypeRepository;
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
            this._customerAttributeService = customerAttributeService;
            this._customerAttributeParser = customerAttributeParser;
            this._customerTagService = customerTagService;
            this._httpContext = httpContext;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region Utilities
        protected IList<CustomerActionType> GetAllCustomerActionType()
        {
            return _cacheManager.Get(CUSTOMER_ACTION_TYPE, () =>
            {                
                return _customerActionTypeRepository.Table.AsQueryable().ToList();
            });
        }



        #region Action
        protected bool UsedAction(string actionId, string customerId)
        {
            var query = from u in _customerActionHistoryRepository.Table
                        where u.CustomerId == customerId && u.CustomerActionId == actionId
                        select u.Id;
            if (query.Count() > 0)
                return true;
            
            return false;
        }

        protected void SaveActionToCustomer(string actionId, string customerId)
        {
            _customerActionHistoryRepository.Insert(new CustomerActionHistory() { CustomerId = customerId, CustomerActionId = actionId, CreateDateUtc = DateTime.UtcNow });
        }
        #endregion

        #region Condition
        protected bool Condition(CustomerAction action, Product product, string attributesXml, Customer customer, string currentUrl, string previousUrl)
        {
            var _cat = GetAllCustomerActionType();
            if (action.Conditions.Count() == 0)
                return true;

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

                #region Action type viewed
                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "Viewed").Id) 
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
                                          where p.CustomerId == customer.Id && p.ActivityLogTypeId == _actLogType.Id
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
                                                           where p.CustomerId == customer.Id && p.ActivityLogTypeId == _actLogType.Id
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
                                                    where p.CustomerId == customer.Id && p.ActivityLogTypeId == _actLogType.Id
                                                    select p.EntityKeyId).Distinct().ToList();
                                cond = ConditionProducts(item, products);
                            }
                        }
                    }
                }
                #endregion

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
                    cond = ConditionCustomerRegister(item, customer);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.CustomCustomerAttribute)
                {
                    cond = ConditionCustomerAttribute(item, customer);
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
            if (condition.Condition == CustomerActionConditionEnum.OneOfThem)
            {
                var attributes = _productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                foreach (var attr in attributes)
                {
                    var attributeValuesStr = _productAttributeParser.ParseValues(AttributesXml, attr.Id);
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
                    var attributes = _productAttributeParser.ParseProductAttributeMappings(product, AttributesXml);
                    if (attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId).Count() > 0)
                    {
                        cond = false;
                        foreach (var attr in attributes.Where(x => x.ProductAttributeId == itemPA.ProductAttributeId))
                        {
                            var attributeValuesStr = _productAttributeParser.ParseValues(AttributesXml, attr.Id);
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

        protected bool ConditionCustomerRegister(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var _genericAttributes = _customerService.GetCustomerById(customer.Id).GenericAttributes;
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
            return cond;
        }

        protected bool ConditionCustomerAttribute(CustomerAction.ActionCondition condition, Customer customer)
        {
            bool cond = false;
            if (customer != null)
            {
                var _genericAttributes = _customerService.GetCustomerById(customer.Id).GenericAttributes;
                if (condition.Condition == CustomerActionConditionEnum.AllOfThem)
                {
                    var customCustomerAttributes = _genericAttributes.FirstOrDefault(x => x.Key == "CustomCustomerAttributes");
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
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
                    if(customCustomerAttributes!=null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
                            foreach (var item in condition.CustomCustomerAttributes)
                            {
                                var _fields = item.RegisterField.Split(':');
                                if(_fields.Count() > 1)
                                {
                                    if (selectedValues.Where(x => x.CustomerAttributeId== _fields.FirstOrDefault() && x.Id == _fields.LastOrDefault()).Count() > 0)
                                        cond = true;
                                }
                            }
                        }
                    }
                }
            }
            return cond;
        }

        #endregion

        #region Reaction
        protected void Reaction(CustomerAction action, Customer customer, ShoppingCartItem cartItem, Order order)
        {
            if (action.ReactionType == CustomerReactionTypeEnum.Banner)
            {
                var banner = _bannerRepository.GetById(action.BannerId);
                if(banner!=null)
                    PrepareBanner(action, banner, customer.Id);
            }

            var _cat = GetAllCustomerActionType();

            if (action.ReactionType == CustomerReactionTypeEnum.Email)
            {
                if (action.ActionTypeId == _cat.FirstOrDefault(x=>x.SystemKeyword== "AddToCart").Id) 
                {
                    if(cartItem!=null)
                        _workflowMessageService.SendCustomerActionEvent_AddToCart_Notification(action, cartItem,
                            _workContext.WorkingLanguage.Id, customer);
                }

                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id) 
                {
                    if(order!=null)
                        _workflowMessageService.SendCustomerActionEvent_AddToOrder_Notification(action, order, customer,
                            _workContext.WorkingLanguage.Id);
                }

                if (action.ActionTypeId != _cat.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id && action.ActionTypeId != _cat.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id) 
                {
                    _workflowMessageService.SendCustomerActionEvent_Notification(action, 
                        _workContext.WorkingLanguage.Id, customer);
                }
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerRole)
            {
                AssignToCustomerRole(action, customer);
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerTag)
            {
                AssignToCustomerTag(action, customer);
            }

            SaveActionToCustomer(action.Id, customer.Id);

        }
        protected void PrepareBanner(CustomerAction action, Banner banner, string customerId)
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

        protected void AssignToCustomerRole(CustomerAction action, Customer customer)
        {
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

        protected void AssignToCustomerTag(CustomerAction action, Customer customer)
        {
            if (customer.CustomerTags.Where(x => x == action.CustomerTagId).Count() == 0)
            {
                _customerTagService.InsertTagToCustomer(action.CustomerTagId, customer.Id);
            }
        }

        #endregion

        #endregion

        #region Methods

        public virtual void AddToCart(ShoppingCartItem cart, Product product, Customer customer)
        {
            Task.Run(() =>
            {
                var actionType = GetAllCustomerActionType().Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddToCart.ToString()).FirstOrDefault();
                if (actionType.Enabled)
                {
                    var datetimeUtcNow = DateTime.UtcNow;
                    var query = from a in _customerActionRepository.Table
                                where a.Active == true && a.ActionTypeId == actionType.Id
                                        && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                select a;

                    foreach (var item in query.ToList())
                    {
                        if (!UsedAction(item.Id, cart.CustomerId))
                        {
                            if (Condition(item, product, cart.AttributesXml, customer, null, null))
                            {
                                Reaction(item, customer, cart, null);
                            }
                        }
                    }
                }
            });
        }

        public virtual void AddOrder(Order order, Customer customer)
        {
            Task.Run(() =>
            {
                var actionType = GetAllCustomerActionType().Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddOrder.ToString()).FirstOrDefault();
                if (actionType.Enabled)
                {
                    var datetimeUtcNow = DateTime.UtcNow;
                    var query = from a in _customerActionRepository.Table
                                where a.Active == true && a.ActionTypeId == actionType.Id
                                        && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                select a;

                    foreach (var item in query.ToList())
                    {
                        Task.Run(() =>
                        {
                            if (!UsedAction(item.Id, order.CustomerId))
                            {
                                foreach (var orderItem in order.OrderItems)
                                {
                                    var product = _productService.GetProductById(orderItem.ProductId);
                                    if (Condition(item, product, orderItem.AttributesXml, customer, null, null))
                                    {
                                        Reaction(item, customer, null, order);
                                        break;
                                    }
                                }
                            }
                        });
                    }

                }
            });
        }

        public virtual void Url(Customer customer, string currentUrl, string previousUrl)
        {
            Task.Run(() =>
            {
                if (!customer.IsSystemAccount)
                {
                    var actionType = GetAllCustomerActionType().Where(x => x.SystemKeyword == CustomerActionTypeEnum.Url.ToString()).FirstOrDefault();
                    if (actionType.Enabled)
                    {
                        var datetimeUtcNow = DateTime.UtcNow;
                        var query = from a in _customerActionRepository.Table
                                    where a.Active == true && a.ActionTypeId == actionType.Id
                                            && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                    select a;

                        foreach (var item in query.ToList())
                        {
                            if (!UsedAction(item.Id, customer.Id))
                            {
                                if (Condition(item, null, null, customer, currentUrl, previousUrl))
                                {
                                    Reaction(item, customer, null, null);
                                }
                            }
                        }

                    }
                }
            });
        }

        public virtual void Viewed(Customer customer, string currentUrl, string previousUrl)
        {
            Task.Run(() =>
            {
                if (!customer.IsSystemAccount)
                {
                    var actionType = GetAllCustomerActionType().Where(x => x.SystemKeyword == CustomerActionTypeEnum.Viewed.ToString()).FirstOrDefault();
                    if (actionType.Enabled)
                    {
                        var datetimeUtcNow = DateTime.UtcNow;
                        var query = from a in _customerActionRepository.Table
                                    where a.Active == true && a.ActionTypeId == actionType.Id
                                            && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                    select a;

                        foreach (var item in query.ToList())
                        {
                            if (!UsedAction(item.Id, customer.Id))
                            {
                                if (Condition(item, null, null, customer, currentUrl, previousUrl))
                                {
                                    Reaction(item, customer, null, null);
                                }
                            }
                        }

                    }
                }
            });
        }
        public virtual void Registration(Customer customer)
        {
            Task.Run(() =>
            {
                var actionType = GetAllCustomerActionType().Where(x => x.SystemKeyword == CustomerActionTypeEnum.Registration.ToString()).FirstOrDefault();
                if (actionType.Enabled)
                {
                    var datetimeUtcNow = DateTime.UtcNow;
                    var query = from a in _customerActionRepository.Table
                                where a.Active == true && a.ActionTypeId == actionType.Id
                                        && datetimeUtcNow >= a.StartDateTimeUtc && datetimeUtcNow <= a.EndDateTimeUtc
                                select a;

                    foreach (var item in query.ToList())
                    {
                        if (!UsedAction(item.Id, customer.Id))
                        {
                            if (Condition(item, null, null, customer, null, null))
                            {
                                Reaction(item, customer, null, null);
                            }
                        }
                    }

                }
            });
        }
        #endregion
    }
}
