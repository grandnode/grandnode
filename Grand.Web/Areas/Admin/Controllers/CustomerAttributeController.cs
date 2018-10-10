using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public CustomerAttributeController(ICustomerAttributeService customerAttributeService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPermissionService permissionService)
        {
            this._customerAttributeService = customerAttributeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._permissionService = permissionService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateAttributeLocales(CustomerAttribute customerAttribute, CustomerAttributeModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name
                    });
            }
            return localized;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateValueLocales(CustomerAttributeValue customerAttributeValue, CustomerAttributeValueModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                    if (!(String.IsNullOrEmpty(local.Name)))
                        localized.Add(new LocalizedProperty()
                        {
                            LanguageId = local.LanguageId,
                            LocaleKey = "Name",
                            LocaleValue = local.Name
                        });
            }
            return localized;
        }

        #endregion
        
        #region Customer attributes

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult ListBlock()
        {
            return PartialView("ListBlock");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //we just redirect a user to the customer settings page
            
            //select second tab
            const int customerFormFieldIndex = 1;
            SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("CustomerUser", "Setting");
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttributes = _customerAttributeService.GetAllCustomerAttributes();
            var gridModel = new DataSourceResult
            {
                Data = customerAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                }),
                Total = customerAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new CustomerAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerAttribute = model.ToEntity();
                customerAttribute.Locales = UpdateAttributeLocales(customerAttribute, model);
                _customerAttributeService.InsertCustomerAttribute(customerAttribute);
                
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(id);
            if (customerAttribute == null)
                //No customer attribute found with the specified id
                return RedirectToAction("List");

            var model = customerAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = customerAttribute.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(model.Id);
            if (customerAttribute == null)
                //No customer attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                customerAttribute = model.ToEntity(customerAttribute);
                customerAttribute.Locales = UpdateAttributeLocales(customerAttribute, model);
                _customerAttributeService.UpdateCustomerAttribute(customerAttribute);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = customerAttribute.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(id);
            _customerAttributeService.DeleteCustomerAttribute(customerAttribute);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Customer attribute values

        //list
        [HttpPost]
        public IActionResult ValueList(string customerAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var values = _customerAttributeService.GetCustomerAttributeById(customerAttributeId).CustomerAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x => new CustomerAttributeValueModel
                {
                    Id = x.Id,
                    CustomerAttributeId = x.CustomerAttributeId,
                    Name = x.Name,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                }),
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string customerAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(customerAttributeId);
            if (customerAttribute == null)
                //No customer attribute found with the specified id
                return RedirectToAction("List");

            var model = new CustomerAttributeValueModel();
            model.CustomerAttributeId = customerAttributeId;
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(CustomerAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var customerAttribute = _customerAttributeService.GetCustomerAttributeById(model.CustomerAttributeId);
            if (customerAttribute == null)
                //No customer attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                var cav = new CustomerAttributeValue
                {
                    CustomerAttributeId = model.CustomerAttributeId,
                    Name = model.Name,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };
                cav.Locales = UpdateValueLocales(cav, model);
                _customerAttributeService.InsertCustomerAttributeValue(cav);
                

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string customerAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            var av = _customerAttributeService.GetCustomerAttributeById(customerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x=>x.Id == id);
            if (cav == null)
                //No customer attribute value found with the specified id
                return RedirectToAction("List");

            var model = new CustomerAttributeValueModel
            {
                CustomerAttributeId = cav.CustomerAttributeId,
                Name = cav.Name,
                IsPreSelected = cav.IsPreSelected,
                DisplayOrder = cav.DisplayOrder
            };

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult ValueEditPopup(CustomerAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var av = _customerAttributeService.GetCustomerAttributeById(model.CustomerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                //No customer attribute value found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                cav.Name = model.Name;
                cav.IsPreSelected = model.IsPreSelected;
                cav.DisplayOrder = model.DisplayOrder;
                cav.Locales = UpdateValueLocales(cav, model);
                _customerAttributeService.UpdateCustomerAttributeValue(cav);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ValueDelete(CustomerAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var av = _customerAttributeService.GetCustomerAttributeById(model.CustomerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                throw new ArgumentException("No customer attribute value found with the specified id");
            _customerAttributeService.DeleteCustomerAttributeValue(cav);

            return new NullJsonResult();
        }


        #endregion
    }
}
