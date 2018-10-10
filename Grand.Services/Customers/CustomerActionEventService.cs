using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Events;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Customers
{
    public partial class CustomerActionEventService : ICustomerActionEventService
    {
        #region Fields
        private const string CUSTOMER_ACTION_TYPE = "Grand.customer.action.type";

        private readonly IRepository<CustomerAction> _customerActionRepository;
        private readonly IRepository<CustomerActionHistory> _customerActionHistoryRepository;
        private readonly IRepository<CustomerActionType> _customerActionTypeRepository;
        private readonly IRepository<Banner> _bannerRepository;
        private readonly IRepository<InteractiveForm> _interactiveFormRepository;
        private readonly IRepository<PopupActive> _popupActiveRepository;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly IPopupService _popupService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CustomerActionEventService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            IRepository<Banner> bannerRepository,
            IRepository<InteractiveForm> interactiveFormRepository,
            IRepository<PopupActive> popupActiveRepository,
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
            IHttpContextAccessor httpContextAccessor,
            ICacheManager cacheManager,
            IPopupService popupService,
            IStoreContext storeContext)
        {
            this._customerActionRepository = customerActionRepository;
            this._customerActionTypeRepository = customerActionTypeRepository;
            this._customerActionHistoryRepository = customerActionHistoryRepository;
            this._bannerRepository = bannerRepository;
            this._interactiveFormRepository = interactiveFormRepository;
            this._popupActiveRepository = popupActiveRepository;
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
            this._httpContextAccessor = httpContextAccessor;
            this._cacheManager = cacheManager;
            this._popupService = popupService;
            this._storeContext = storeContext;
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
                        if (_actLogType != null)
                        {
                            if (_actLogType.Enabled)
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
                    cond = item.UrlCurrent.Select(x => x.Name).Contains(currentUrl);
                }

                if (item.CustomerActionConditionType == CustomerActionConditionTypeEnum.UrlReferrer)
                {
                    cond = item.UrlReferrer.Select(x => x.Name).Contains(previousUrl);
                }

                if(item.CustomerActionConditionType == CustomerActionConditionTypeEnum.Store)
                {
                    cond = ConditionStores(item, _storeContext.CurrentStore.Id);
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
                    if (customCustomerAttributes != null)
                    {
                        if (!String.IsNullOrEmpty(customCustomerAttributes.Value))
                        {
                            var selectedValues = _customerAttributeParser.ParseCustomerAttributeValues(customCustomerAttributes.Value);
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

        #endregion

        #region Reaction
        public void Reaction(CustomerAction action, Customer customer, ShoppingCartItem cartItem, Order order)
        {
            if (action.ReactionType == CustomerReactionTypeEnum.Banner)
            {
                var banner = _bannerRepository.GetById(action.BannerId);
                if (banner != null)
                    PrepareBanner(action, banner, customer.Id);
            }
            if (action.ReactionType == CustomerReactionTypeEnum.InteractiveForm)
            {
                var interactiveform = _interactiveFormRepository.GetById(action.InteractiveFormId);
                if (interactiveform != null)
                    PrepareInteractiveForm(action, interactiveform, customer.Id);
            }

            var _cat = GetAllCustomerActionType();

            if (action.ReactionType == CustomerReactionTypeEnum.Email)
            {
                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id)
                {
                    if (cartItem != null)
                        _workflowMessageService.SendCustomerActionEvent_AddToCart_Notification(action, cartItem,
                            _workContext.WorkingLanguage.Id, customer);
                }

                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id)
                {
                    if (order != null)
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
            var banneractive = new PopupActive()
            {
                Body = banner.Body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = banner.Name,
                PopupTypeId = (int)PopupType.Banner
            };
            _popupService.InsertPopupActive(banneractive);
        }

        protected void PrepareInteractiveForm(CustomerAction action, InteractiveForm form, string customerId)
        {

            var body = PrepareDataInteractiveForm(form);

            var formactive = new PopupActive()
            {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = form.Name,
                PopupTypeId = (int)PopupType.InteractiveForm
            };
            _popupService.InsertPopupActive(formactive);
        }

        protected string PrepareDataInteractiveForm(InteractiveForm form)
        {
            var body = form.Body;
            body += "<input type=\"hidden\" name=\"Id\" value=\"" + form.Id + "\">";
            foreach (var item in form.FormAttributes)
            {
                if (item.AttributeControlType == FormControlType.TextBox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textbox = string.Format("<input type=\"text\"  name=\"{0}\" class=\"{1}\" style=\"{2}\" value=\"{3}\" {4}>", item.SystemName, _class, _style ,_value, item.IsRequired ? "required" : "");
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textbox);
                }
                if (item.AttributeControlType == FormControlType.MultilineTextbox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textarea = string.Format("<textarea name=\"{0}\" class=\"{1}\" style=\"{2}\" {3}> {4} </textarea>", item.SystemName, _class, _style, item.IsRequired ? "required" : "", _value);
                    body = body.Replace(string.Format("%{0}%", item.SystemName), textarea);
                }
                if (item.AttributeControlType == FormControlType.Checkboxes)
                {
                    var checkbox = "<div class=\"custom-controls-stacked\">";
                    foreach (var itemcheck in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        checkbox += "<label class=\"custom-control custom-checkbox\">";
                        checkbox += string.Format("<input type=\"checkbox\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\" >", _class, _style, 
                            itemcheck.IsPreSelected ? "checked" : "", itemcheck.Id, item.SystemName, itemcheck.GetLocalized(x => x.Name));
                        checkbox += "<span class=\"custom-control-indicator\"></span>";
                        checkbox += string.Format("<span class=\"custom-control-description\">{0}</span>", itemcheck.GetLocalized(x => x.Name));
                        checkbox += "</label>";
                    }
                    checkbox += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), checkbox);
                }

                if (item.AttributeControlType == FormControlType.DropdownList)
                {
                    var dropdown = string.Empty;
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control custom-select", item.Class);

                    dropdown = string.Format("<select name=\"{0}\" class=\"{1}\" style=\"{2}\" >", item.SystemName, _class, _style);
                    foreach (var itemdropdown in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        dropdown += string.Format("<option value=\"{0}\" {1}>{2}</option>", itemdropdown.GetLocalized(x => x.Name), itemdropdown.IsPreSelected ? "selected" : "", itemdropdown.GetLocalized(x => x.Name));
                    }
                    dropdown += "</select>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), dropdown);
                }
                if (item.AttributeControlType == FormControlType.RadioList)
                {
                    var radio = "<div class=\"custom-controls-stacked\">";
                    foreach (var itemradio in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        string _style = string.Format("{0}", item.Style);
                        string _class = string.Format("{0} {1}", "custom-control-input", item.Class);

                        radio += "<label class=\"custom-control custom-radio\">";
                        radio += string.Format("<input type=\"radio\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\">", _class, _style,
                            itemradio.IsPreSelected ? "checked" : "", itemradio.Id, item.SystemName, itemradio.GetLocalized(x => x.Name));
                        radio += "<span class=\"custom-control-indicator\"></span>";
                        radio += string.Format("<span class=\"custom-control-description\">{0}</span>", itemradio.GetLocalized(x => x.Name));
                        radio += "</label>";
                    }
                    radio += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), radio);
                }
            }

            return body;
        }

        protected void AssignToCustomerRole(CustomerAction action, Customer customer)
        {
            if (customer.CustomerRoles.Where(x => x.Id == action.CustomerRoleId).Count() == 0)
            {
                var customerRole = _customerService.GetCustomerRoleById(action.CustomerRoleId);
                if (customerRole != null)
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
                    if (!UsedAction(item.Id, customer.Id))
                    {
                        if (Condition(item, product, cart.AttributesXml, customer, null, null))
                        {
                            Reaction(item, customer, cart, null);
                        }
                    }
                }
            }
        }

        public virtual void AddOrder(Order order, Customer customer)
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
                }

            }
            
        }

        public virtual void Url(Customer customer, string currentUrl, string previousUrl)
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
        }

        public virtual void Viewed(Customer customer, string currentUrl, string previousUrl)
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
            
        }
        public virtual void Registration(Customer customer)
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
        }
        #endregion
    }
}
