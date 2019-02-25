using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Settings)]
    public partial class CustomerAttributeController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerAttributeViewModelService _customerAttributeViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public CustomerAttributeController(ICustomerAttributeService customerAttributeService,
            ICustomerAttributeViewModelService customerAttributeViewModelService,
            ILanguageService languageService,
            ILocalizationService localizationService)
        {
            this._customerAttributeService = customerAttributeService;
            this._customerAttributeViewModelService = customerAttributeViewModelService;
            this._languageService = languageService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Customer attributes

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult ListBlock() => PartialView("ListBlock");

        public IActionResult List()
        {
            //we just redirect a user to the customer settings page
            //select second tab
            const int customerFormFieldIndex = 1;
            SaveSelectedTabIndex(customerFormFieldIndex);
            return RedirectToAction("CustomerUser", "Setting");
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
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
            var model = _customerAttributeViewModelService.PrepareCustomerAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerAttributeModel model, bool continueEditing)
        {
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

                    return RedirectToAction("Edit", new { id = customerAttribute.Id });
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
            var av = _customerAttributeService.GetCustomerAttributeById(customerAttributeId);
            var cav = av.CustomerAttributeValues.FirstOrDefault(x => x.Id == id);
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
            _customerAttributeViewModelService.DeleteCustomerAttributeValue(model);

            return new NullJsonResult();
        }
        #endregion
    }
}
