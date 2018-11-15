using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeViewModelService _customerAttributeViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public CustomerAttributeController(ICustomerAttributeService customerAttributeService,
            ICustomerAttributeViewModelService customerAttributeViewModelService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            this._customerAttributeService = customerAttributeService;
            this._customerAttributeViewModelService = customerAttributeViewModelService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
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

            var customerAttributes = _customerAttributeViewModelService.PrepareCustomerAttributes();
            var gridModel = new DataSourceResult
            {
                Data = customerAttributes.ToList(),
                Total = customerAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _customerAttributeViewModelService.PrepareCustomerAttributeModel();
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
                var customerAttribute = _customerAttributeViewModelService.InsertCustomerAttributeModel(model);
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

            var model = _customerAttributeViewModelService.PrepareCustomerAttributeModel(customerAttribute);
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
                customerAttribute = _customerAttributeViewModelService.UpdateCustomerAttributeModel(model, customerAttribute);

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

            _customerAttributeViewModelService.DeleteCustomerAttribute(id);

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

            var values = _customerAttributeViewModelService.PrepareCustomerAttributeValues(customerAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = values.ToList(),
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

            var model = _customerAttributeViewModelService.PrepareCustomerAttributeValueModel(customerAttributeId);
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
                _customerAttributeViewModelService.InsertCustomerAttributeValueModel(model);
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

            var model = _customerAttributeViewModelService.PrepareCustomerAttributeValueModel(cav);
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
                _customerAttributeViewModelService.UpdateCustomerAttributeValueModel(model, cav);

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

            _customerAttributeViewModelService.DeleteCustomerAttributeValue(model);

            return new NullJsonResult();
        }


        #endregion
    }
}
