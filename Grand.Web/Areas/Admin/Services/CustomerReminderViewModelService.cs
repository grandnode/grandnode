using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CustomerReminderViewModelService : ICustomerReminderViewModelService
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerTagService _customerTagService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerReminderService _customerReminderService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #region Constructors

        public CustomerReminderViewModelService(ICustomerService customerService,
            ICustomerTagService customerTagService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            ICustomerReminderService customerReminderService,
            IEmailAccountService emailAccountService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerService = customerService;
            _customerTagService = customerTagService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _customerReminderService = customerReminderService;
            _emailAccountService = emailAccountService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities
        #endregion
        public virtual CustomerReminderModel PrepareCustomerReminderModel()
        {
            var model = new CustomerReminderModel();
            model.StartDateTime = DateTime.Now;
            model.EndDateTime = DateTime.Now.AddMonths(1);
            model.LastUpdateDate = DateTime.Now.AddDays(-7);
            model.Active = true;
            return model;
        }

        public virtual CustomerReminder InsertCustomerReminderModel(CustomerReminderModel model)
        {
            var customerreminder = model.ToEntity();
            _customerReminderService.InsertCustomerReminder(customerreminder);
            //activity log
            _customerActivityService.InsertActivity("AddNewCustomerReminder", customerreminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminder"), customerreminder.Name);
            return customerreminder;
        }
        public virtual CustomerReminder UpdateCustomerReminderModel(CustomerReminder customerReminder, CustomerReminderModel model)
        {
            if (customerReminder.Conditions.Count() > 0)
                model.ReminderRuleId = customerReminder.ReminderRuleId;
            if (model.ReminderRuleId == 0)
                model.ReminderRuleId = customerReminder.ReminderRuleId;

            customerReminder = model.ToEntity(customerReminder);
            _customerReminderService.UpdateCustomerReminder(customerReminder);
            _customerActivityService.InsertActivity("EditCustomerReminder", customerReminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminder"), customerReminder.Name);
            return customerReminder;
        }
        public virtual void RunReminder(CustomerReminder customerReminder)
        {
            if (customerReminder.ReminderRule == CustomerReminderRuleEnum.AbandonedCart)
                _customerReminderService.Task_AbandonedCart(customerReminder.Id);

            if (customerReminder.ReminderRule == CustomerReminderRuleEnum.Birthday)
                _customerReminderService.Task_Birthday(customerReminder.Id);

            if (customerReminder.ReminderRule == CustomerReminderRuleEnum.LastActivity)
                _customerReminderService.Task_LastActivity(customerReminder.Id);

            if (customerReminder.ReminderRule == CustomerReminderRuleEnum.LastPurchase)
                _customerReminderService.Task_LastPurchase(customerReminder.Id);

            if (customerReminder.ReminderRule == CustomerReminderRuleEnum.RegisteredCustomer)
                _customerReminderService.Task_RegisteredCustomer(customerReminder.Id);
        }
        public virtual void DeleteCustomerReminder(CustomerReminder customerReminder)
        {
            _customerActivityService.InsertActivity("DeleteCustomerReminder", customerReminder.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerReminder"), customerReminder.Name);
            _customerReminderService.DeleteCustomerReminder(customerReminder);

        }

        public virtual SerializeCustomerReminderHistoryModel PrepareHistoryModelForList(SerializeCustomerReminderHistory history)
        {
            return new SerializeCustomerReminderHistoryModel
            {
                Id = history.Id,
                Email = _customerService.GetCustomerById(history.CustomerId).Email,
                SendDate = _dateTimeHelper.ConvertToUserTime(history.SendDate, DateTimeKind.Utc),
                Level = history.Level,
                OrderId = !String.IsNullOrEmpty(history.OrderId)
            };
        }

        public virtual CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder)
        {
            var model = new CustomerReminderModel.ConditionModel();
            model.CustomerReminderId = customerReminder.Id;
            foreach (CustomerReminderConditionTypeEnum item in Enum.GetValues(typeof(CustomerReminderConditionTypeEnum)))
            {
                if (customerReminder.ReminderRule == CustomerReminderRuleEnum.AbandonedCart || customerReminder.ReminderRule == CustomerReminderRuleEnum.CompletedOrder
                    || customerReminder.ReminderRule == CustomerReminderRuleEnum.UnpaidOrder)
                    model.ConditionType.Add(new SelectListItem()
                    {
                        Value = ((int)item).ToString(),
                        Text = item.ToString()
                    });
                else
                {
                    if (item != CustomerReminderConditionTypeEnum.Product &&
                        item != CustomerReminderConditionTypeEnum.Manufacturer &&
                        item != CustomerReminderConditionTypeEnum.Category)
                    {
                        model.ConditionType.Add(new SelectListItem()
                        {
                            Value = ((int)item).ToString(),
                            Text = item.ToString()
                        });

                    }
                }
            }
            return model;
        }
        public virtual CustomerReminder.ReminderCondition InsertConditionModel(CustomerReminder customerReminder, CustomerReminderModel.ConditionModel model)
        {
            var condition = new CustomerReminder.ReminderCondition()
            {
                Name = model.Name,
                ConditionTypeId = model.ConditionTypeId,
                ConditionId = model.ConditionId,
            };
            customerReminder.Conditions.Add(condition);
            _customerReminderService.UpdateCustomerReminder(customerReminder);

            _customerActivityService.InsertActivity("AddNewCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminder"), customerReminder.Name);

            return condition;
        }

        public virtual CustomerReminderModel.ConditionModel PrepareConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition)
        {
            var model = condition.ToModel();
            model.CustomerReminderId = customerReminder.Id;
            foreach (CustomerReminderConditionTypeEnum item in Enum.GetValues(typeof(CustomerReminderConditionTypeEnum)))
            {
                model.ConditionType.Add(new SelectListItem()
                {
                    Value = ((int)item).ToString(),
                    Text = item.ToString()
                });
            }
            return model;
        }
        public virtual CustomerReminder.ReminderCondition UpdateConditionModel(CustomerReminder customerReminder, CustomerReminder.ReminderCondition condition, CustomerReminderModel.ConditionModel model)
        {
            condition = model.ToEntity(condition);
            _customerReminderService.UpdateCustomerReminder(customerReminder);
            _customerActivityService.InsertActivity("EditCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminderCondition"), customerReminder.Name);
            return condition;
        }
        public virtual void ConditionDelete(string Id, string customerReminderId)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == Id);
                if (condition != null)
                {
                    customerReminder.Conditions.Remove(condition);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public void ConditionDeletePosition(string id, string customerReminderId, string conditionId)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Product)
            {
                condition.Products.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Category)
            {
                condition.Categories.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Manufacturer)
            {
                condition.Manufacturers.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerRole)
            {
                condition.CustomerRoles.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerTag)
            {
                condition.CustomerTags.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomCustomerAttribute)
            {
                condition.CustomCustomerAttributes.Remove(condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == id));
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerRegisterField)
            {
                condition.CustomerRegistration.Remove(condition.CustomerRegistration.FirstOrDefault(x => x.Id == id));
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
        }
        public virtual void InsertCategoryConditionModel(CustomerReminderModel.ConditionModel.AddCategoryConditionModel model)
        {
            foreach (string id in model.SelectedCategoryIds)
            {
                var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder != null)
                {
                    var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                    if (condition != null)
                    {
                        if (condition.Categories.Where(x => x == id).Count() == 0)
                        {
                            condition.Categories.Add(id);
                            _customerReminderService.UpdateCustomerReminder(customerReminder);
                        }
                    }
                }
            }
        }
        public virtual void InsertManufacturerConditionModel(CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model)
        {
            foreach (string id in model.SelectedManufacturerIds)
            {
                var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder != null)
                {
                    var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                    if (condition != null)
                    {
                        if (condition.Manufacturers.Where(x => x == id).Count() == 0)
                        {
                            condition.Manufacturers.Add(id);
                            _customerReminderService.UpdateCustomerReminder(customerReminder);
                        }
                    }
                }
            }
        }
        public virtual void InsertProductToConditionModel(CustomerReminderModel.ConditionModel.AddProductToConditionModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder != null)
                {
                    var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                    if (condition != null)
                    {
                        if (condition.Products.Where(x => x == id).Count() == 0)
                        {
                            condition.Products.Add(id);
                            _customerReminderService.UpdateCustomerReminder(customerReminder);
                        }
                    }
                }
            }
        }
        public virtual void InsertCustomerTagConditionModel(CustomerReminderModel.ConditionModel.AddCustomerTagConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerTags.Where(x => x == model.CustomerTagId).Count() == 0)
                    {
                        condition.CustomerTags.Add(model.CustomerTagId);
                        _customerReminderService.UpdateCustomerReminder(customerReminder);
                    }
                }
            }
        }
        public virtual void InsertCustomerRoleConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRoleConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerRoles.Where(x => x == model.CustomerRoleId).Count() == 0)
                    {
                        condition.CustomerRoles.Add(model.CustomerRoleId);
                        _customerReminderService.UpdateCustomerReminder(customerReminder);
                    }
                }
            }
        }
        public virtual void InsertCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerReminder.ReminderCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerRegisterName,
                        RegisterValue = model.CustomerRegisterValue,
                    };
                    condition.CustomerRegistration.Add(_cr);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public virtual void UpdateCustomerRegisterConditionModel(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomerRegistration.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerRegisterName;
                    cr.RegisterValue = model.CustomerRegisterValue;
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public virtual void InsertCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerReminder.ReminderCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerAttributeName,
                        RegisterValue = model.CustomerAttributeValue,
                    };
                    condition.CustomCustomerAttributes.Add(_cr);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public virtual void UpdateCustomCustomerAttributeConditionModel(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerAttributeName;
                    cr.RegisterValue = model.CustomerAttributeValue;
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public virtual void PrepareReminderLevelModel(CustomerReminderModel.ReminderLevelModel model, CustomerReminder customerReminder)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var emailAccounts = _emailAccountService.GetAllEmailAccounts();
            foreach (var item in emailAccounts)
            {
                model.EmailAccounts.Add(new SelectListItem
                {
                    Text = item.Email,
                    Value = item.Id.ToString()
                });
            }
            var messageTokenProvider = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IMessageTokenProvider>();
            model.AllowedTokens = messageTokenProvider.GetListOfCustomerReminderAllowedTokens(customerReminder.ReminderRule);
        }
        public virtual CustomerReminder.ReminderLevel InsertReminderLevel(CustomerReminder customerReminder, CustomerReminderModel.ReminderLevelModel model)
        {
            var level = new CustomerReminder.ReminderLevel()
            {
                Name = model.Name,
                Level = model.Level,
                BccEmailAddresses = model.BccEmailAddresses,
                Body = model.Body,
                EmailAccountId = model.EmailAccountId,
                Subject = model.Subject,
                Day = model.Day,
                Hour = model.Hour,
                Minutes = model.Minutes,
            };

            customerReminder.Levels.Add(level);
            _customerReminderService.UpdateCustomerReminder(customerReminder);
            _customerActivityService.InsertActivity("AddNewCustomerReminderLevel", customerReminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminderLevel"), customerReminder.Name);
            return level;
        }
        public virtual CustomerReminder.ReminderLevel UpdateReminderLevel(CustomerReminder customerReminder, CustomerReminder.ReminderLevel customerReminderLevel, CustomerReminderModel.ReminderLevelModel model)
        {
            customerReminderLevel.Level = model.Level;
            customerReminderLevel.Name = model.Name;
            customerReminderLevel.Subject = model.Subject;
            customerReminderLevel.BccEmailAddresses = model.BccEmailAddresses;
            customerReminderLevel.Body = model.Body;
            customerReminderLevel.EmailAccountId = model.EmailAccountId;
            customerReminderLevel.Day = model.Day;
            customerReminderLevel.Hour = model.Hour;
            customerReminderLevel.Minutes = model.Minutes;
            _customerReminderService.UpdateCustomerReminder(customerReminder);
            _customerActivityService.InsertActivity("EditCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminderLevel"), customerReminder.Name);
            return customerReminderLevel;
        }
        public virtual void DeleteLevel(string Id, string customerReminderId)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder != null)
            {
                var level = customerReminder.Levels.FirstOrDefault(x => x.Id == Id);
                if (level != null)
                {
                    customerReminder.Levels.Remove(level);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerActionConditionModel.AddProductToConditionModel model, int pageIndex, int pageSize)
        {
            var productService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IProductService>();
            var products = productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual CustomerReminderModel.ConditionModel.AddProductToConditionModel PrepareProductToConditionModel(string customerReminderId, string conditionId)
        {
            var categoryService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<ICategoryService>();
            var manufacturerService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IManufacturerService>();
            var storeService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IStoreService>();
            var vendorService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IVendorService>();

            var model = new CustomerReminderModel.ConditionModel.AddProductToConditionModel();
            model.ConditionId = conditionId;
            model.CustomerReminderId = customerReminderId;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }
    }
}
