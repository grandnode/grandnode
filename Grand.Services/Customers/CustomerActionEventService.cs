using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Logging;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IPopupService _popupService;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Ctor

        public CustomerActionEventService(IRepository<CustomerAction> customerActionRepository,
            IRepository<CustomerActionType> customerActionTypeRepository,
            IRepository<CustomerActionHistory> customerActionHistoryRepository,
            IRepository<Banner> bannerRepository,
            IRepository<InteractiveForm> interactiveFormRepository,
            IRepository<ActivityLog> activityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IPopupService popupService,
            IServiceProvider serviceProvider)
        {
            _customerActionRepository = customerActionRepository;
            _customerActionTypeRepository = customerActionTypeRepository;
            _customerActionHistoryRepository = customerActionHistoryRepository;
            _bannerRepository = bannerRepository;
            _interactiveFormRepository = interactiveFormRepository;
            _activityLogRepository = activityLogRepository;
            _activityLogTypeRepository = activityLogTypeRepository;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _popupService = popupService;
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Utilities

        protected async Task<IList<CustomerActionType>> GetAllCustomerActionType()
        {
            return await _cacheManager.GetAsync(CUSTOMER_ACTION_TYPE, () =>
            {
                return _customerActionTypeRepository.Table.ToListAsync();
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

        protected async Task SaveActionToCustomer(string actionId, string customerId)
        {
            await _customerActionHistoryRepository.InsertAsync(new CustomerActionHistory() { CustomerId = customerId, CustomerActionId = actionId, CreateDateUtc = DateTime.UtcNow });
        }
        #endregion

        #region Condition
        protected async Task<bool> Condition(CustomerAction action, Product product, string attributesXml, Customer customer, string currentUrl, string previousUrl)
        {
            var _cat = await GetAllCustomerActionType();
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

        #endregion

        #region Reaction
        public async Task Reaction(CustomerAction action, Customer customer, ShoppingCartItem cartItem, Order order)
        {
            if (action.ReactionType == CustomerReactionTypeEnum.Banner)
            {
                var banner = await _bannerRepository.GetByIdAsync(action.BannerId);
                if (banner != null)
                    await PrepareBanner(action, banner, customer.Id);
            }
            if (action.ReactionType == CustomerReactionTypeEnum.InteractiveForm)
            {
                var interactiveform = await _interactiveFormRepository.GetByIdAsync(action.InteractiveFormId);
                if (interactiveform != null)
                    await PrepareInteractiveForm(action, interactiveform, customer.Id);
            }

            var _cat = await GetAllCustomerActionType();

            if (action.ReactionType == CustomerReactionTypeEnum.Email)
            {
                var workflowMessageService = _serviceProvider.GetRequiredService<IWorkflowMessageService>();
                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id)
                {
                    if (cartItem != null)
                        await workflowMessageService.SendCustomerActionEvent_AddToCart_Notification(action, cartItem,
                            _workContext.WorkingLanguage.Id, customer);
                }

                if (action.ActionTypeId == _cat.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id)
                {
                    if (order != null)
                        await workflowMessageService.SendCustomerActionEvent_AddToOrder_Notification(action, order, customer,
                            _workContext.WorkingLanguage.Id);
                }

                if (action.ActionTypeId != _cat.FirstOrDefault(x => x.SystemKeyword == "AddOrder").Id && action.ActionTypeId != _cat.FirstOrDefault(x => x.SystemKeyword == "AddToCart").Id)
                {
                    await workflowMessageService.SendCustomerActionEvent_Notification(action,
                        _workContext.WorkingLanguage.Id, customer);
                }
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerRole)
            {
                await AssignToCustomerRole(action, customer);
            }

            if (action.ReactionType == CustomerReactionTypeEnum.AssignToCustomerTag)
            {
                await AssignToCustomerTag(action, customer);
            }

            await SaveActionToCustomer(action.Id, customer.Id);

        }
        protected async Task PrepareBanner(CustomerAction action, Banner banner, string customerId)
        {
            var banneractive = new PopupActive() {
                Body = banner.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id),
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = banner.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                PopupTypeId = (int)PopupType.Banner
            };
            await _popupService.InsertPopupActive(banneractive);
        }

        protected async Task PrepareInteractiveForm(CustomerAction action, InteractiveForm form, string customerId)
        {

            var body = PrepareDataInteractiveForm(form);

            var formactive = new PopupActive() {
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = customerId,
                CustomerActionId = action.Id,
                Name = form.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                PopupTypeId = (int)PopupType.InteractiveForm
            };
            await _popupService.InsertPopupActive(formactive);
        }

        protected string PrepareDataInteractiveForm(InteractiveForm form)
        {
            var body = form.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id);
            body += "<input type=\"hidden\" name=\"Id\" value=\"" + form.Id + "\">";
            foreach (var item in form.FormAttributes)
            {
                if (item.AttributeControlType == FormControlType.TextBox)
                {
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control", item.Class);
                    string _value = item.DefaultValue;
                    var textbox = string.Format("<input type=\"text\"  name=\"{0}\" class=\"{1}\" style=\"{2}\" value=\"{3}\" {4}>", item.SystemName, _class, _style, _value, item.IsRequired ? "required" : "");
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

                        checkbox += "<div class=\"custom-control custom-checkbox\">";
                        checkbox += string.Format("<input type=\"checkbox\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\">", _class, _style,
                            itemcheck.IsPreSelected ? "checked" : "", itemcheck.Id, item.SystemName, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        checkbox += string.Format("<label class=\"custom-control-label\" for=\"{0}\">{1}</label>", itemcheck.Id, itemcheck.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        checkbox += "</div>";
                    }
                    checkbox += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), checkbox);
                }

                if (item.AttributeControlType == FormControlType.DropdownList)
                {
                    var dropdown = string.Empty;
                    string _style = string.Format("{0}", item.Style);
                    string _class = string.Format("{0} {1}", "form-control custom-select", item.Class);

                    dropdown = string.Format("<select name=\"{0}\" class=\"{1}\" style=\"{2}\">", item.SystemName, _class, _style);
                    foreach (var itemdropdown in item.FormAttributeValues.OrderBy(x => x.DisplayOrder))
                    {
                        dropdown += string.Format("<option value=\"{0}\" {1}>{2}</option>", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), itemdropdown.IsPreSelected ? "selected" : "", itemdropdown.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
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

                        radio += "<div class=\"custom-control custom-radio\">";
                        radio += string.Format("<input type=\"radio\" class=\"{0}\" style=\"{1}\" {2} id=\"{3}\" name=\"{4}\" value=\"{5}\">", _class, _style,
                            itemradio.IsPreSelected ? "checked" : "", itemradio.Id, item.SystemName, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += string.Format("<label class=\"custom-control-label\" for=\"{0}\">{1}</label>", itemradio.Id, itemradio.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                        radio += "</div>";
                    }
                    radio += "</div>";
                    body = body.Replace(string.Format("%{0}%", item.SystemName), radio);
                }
            }
            body = body.Replace("%sendbutton%", "<input type=\"submit\" id=\"send-interactive-form\" class=\"btn btn-success interactive-form-button\" value=\"Send\" \" />");
            body = body.Replace("%errormessage%", "<div class=\"message-error\"><div class=\"validation-summary-errors\"><div id=\"errorMessages\"></div></div></div>");

            return body;
        }

        protected async Task AssignToCustomerRole(CustomerAction action, Customer customer)
        {
            if (customer.CustomerRoles.Where(x => x.Id == action.CustomerRoleId).Count() == 0)
            {
                var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                var customerRole = await customerService.GetCustomerRoleById(action.CustomerRoleId);
                if (customerRole != null)
                {
                    customerRole.CustomerId = customer.Id;
                    await customerService.InsertCustomerRoleInCustomer(customerRole);
                }
            }
        }

        protected async Task AssignToCustomerTag(CustomerAction action, Customer customer)
        {
            if (customer.CustomerTags.Where(x => x == action.CustomerTagId).Count() == 0)
            {
                var customerTagService = _serviceProvider.GetRequiredService<ICustomerTagService>();
                await customerTagService.InsertTagToCustomer(action.CustomerTagId, customer.Id);
            }
        }

        #endregion

        #endregion

        #region Methods

        public virtual async Task AddToCart(ShoppingCartItem cart, Product product, Customer customer)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.AddToCart.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
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
                        if (await Condition(item, product, cart.AttributesXml, customer, null, null))
                        {
                            await Reaction(item, customer, cart, null);
                        }
                    }
                }
            }
        }

        public virtual async Task AddOrder(Order order, CustomerActionTypeEnum customerActionType)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == customerActionType.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
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
                        var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                        var productService = _serviceProvider.GetRequiredService<IProductService>();
                        var customer = await customerService.GetCustomerById(order.CustomerId);
                        foreach (var orderItem in order.OrderItems)
                        {
                            var product = await productService.GetProductById(orderItem.ProductId);
                            if (await Condition(item, product, orderItem.AttributesXml, customer, null, null))
                            {
                                await Reaction(item, customer, null, order);
                                break;
                            }
                        }
                    }
                }

            }

        }

        public virtual async Task Url(Customer customer, string currentUrl, string previousUrl)
        {
            if (!customer.IsSystemAccount)
            {
                var actiontypes = await GetAllCustomerActionType();
                var actionType = actiontypes.FirstOrDefault(x => x.SystemKeyword == CustomerActionTypeEnum.Url.ToString());
                if (actionType?.Enabled == true)
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
                            if (await Condition(item, null, null, customer, currentUrl, previousUrl))
                            {
                                await Reaction(item, customer, null, null);
                            }
                        }
                    }
                }
            }
        }

        public virtual async Task Viewed(Customer customer, string currentUrl, string previousUrl)
        {
            if (!customer.IsSystemAccount)
            {
                var actiontypes = await GetAllCustomerActionType();
                var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Viewed.ToString()).FirstOrDefault();
                if (actionType?.Enabled == true)
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
                            if (await Condition(item, null, null, customer, currentUrl, previousUrl))
                            {
                                await Reaction(item, customer, null, null);
                            }
                        }
                    }

                }
            }

        }
        public virtual async Task Registration(Customer customer)
        {
            var actiontypes = await GetAllCustomerActionType();
            var actionType = actiontypes.Where(x => x.SystemKeyword == CustomerActionTypeEnum.Registration.ToString()).FirstOrDefault();
            if (actionType?.Enabled == true)
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
                        if (await Condition(item, null, null, customer, null, null))
                        {
                            await Reaction(item, customer, null, null);
                        }
                    }
                }

            }
        }
        #endregion
    }
}
