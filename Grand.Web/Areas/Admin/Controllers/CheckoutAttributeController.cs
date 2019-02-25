using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Attributes)]
    public partial class CheckoutAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly ICheckoutAttributeViewModelService _checkoutAttributeViewModelService;

        #endregion

        #region Constructors

        public CheckoutAttributeController(ICheckoutAttributeService checkoutAttributeService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            ICurrencyService currencyService, 
            CurrencySettings currencySettings,
            IMeasureService measureService, 
            MeasureSettings measureSettings,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            ICheckoutAttributeViewModelService checkoutAttributeViewModelService)
        {
            this._checkoutAttributeService = checkoutAttributeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
            this._checkoutAttributeViewModelService = checkoutAttributeViewModelService;
        }

        #endregion
        
        #region Checkout attributes

        //list
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var checkoutAttributes = _checkoutAttributeViewModelService.PrepareCheckoutAttributeListModel();
            var gridModel = new DataSourceResult
            {
                Data = checkoutAttributes.ToList(),
                Total = checkoutAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            var model = _checkoutAttributeViewModelService.PrepareCheckoutAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //ACL
            model.PrepareACLModel(null, false, _customerService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CheckoutAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var checkoutAttribute = _checkoutAttributeViewModelService.InsertCheckoutAttributeModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = checkoutAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //tax categories
            _checkoutAttributeViewModelService.PrepareTaxCategories(model, null, true);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //ACL
            model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            if (checkoutAttribute == null)
                //No checkout attribute found with the specified id
                return RedirectToAction("List");

            var model = checkoutAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = checkoutAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.TextPrompt = checkoutAttribute.GetLocalized(x => x.TextPrompt, languageId, false, false);
            });

            //tax categories
            _checkoutAttributeViewModelService.PrepareTaxCategories(model, checkoutAttribute, false);
            //Stores
            model.PrepareStoresMappingModel(checkoutAttribute, false, _storeService);

            //condition
            _checkoutAttributeViewModelService.PrepareConditionAttributes(model, checkoutAttribute);
            //ACL
            model.PrepareACLModel(checkoutAttribute, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(checkoutAttribute, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CheckoutAttributeModel model, bool continueEditing)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.Id);
            if (checkoutAttribute == null)
                //No checkout attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                checkoutAttribute = _checkoutAttributeViewModelService.UpdateCheckoutAttributeModel(checkoutAttribute, model);
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = checkoutAttribute.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //tax categories
            _checkoutAttributeViewModelService.PrepareTaxCategories(model, checkoutAttribute, true);
            //Stores
            model.PrepareStoresMappingModel(checkoutAttribute, true, _storeService);
            //ACL
            model.PrepareACLModel(checkoutAttribute, true, _customerService);

            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id, [FromServices] ICustomerActivityService customerActivityService)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            _checkoutAttributeService.DeleteCheckoutAttribute(checkoutAttribute);

            //activity log
            customerActivityService.InsertActivity("DeleteCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Checkout attribute values

        //list
        [HttpPost]
        public IActionResult ValueList(string checkoutAttributeId, DataSourceRequest command)
        {
            var checkoutAttribute = _checkoutAttributeViewModelService.PrepareCheckoutAttributeValuesModel(checkoutAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = checkoutAttribute.ToList(),
                Total = checkoutAttribute.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string checkoutAttributeId)
        {
            var model = _checkoutAttributeViewModelService.PrepareCheckoutAttributeValueModel(checkoutAttributeId);
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(CheckoutAttributeValueModel model)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.CheckoutAttributeId);
            if (checkoutAttribute == null)
                //No checkout attribute found with the specified id
                return RedirectToAction("List");

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            if (checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");                
            }

            if (ModelState.IsValid)
            {
                var cav = _checkoutAttributeViewModelService.InsertCheckoutAttributeValueModel(checkoutAttribute, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string checkoutAttributeId)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var cav = checkoutAttribute.CheckoutAttributeValues.Where(x=>x.Id == id).FirstOrDefault();
            if (cav == null)
                //No checkout attribute value found with the specified id
                return RedirectToAction("List");

            var model = _checkoutAttributeViewModelService.PrepareCheckoutAttributeValueModel(checkoutAttribute, cav);

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult ValueEditPopup(CheckoutAttributeValueModel model)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.CheckoutAttributeId);

            var cav = checkoutAttribute.CheckoutAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (cav == null)
                //No checkout attribute value found with the specified id
                return RedirectToAction("List");

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            if (checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            if (ModelState.IsValid)
            {
                _checkoutAttributeViewModelService.UpdateCheckoutAttributeValueModel(checkoutAttribute, cav, model);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ValueDelete(string id, string checkoutAttributeId)
        {
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var cav = checkoutAttribute.CheckoutAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (cav == null)
                throw new ArgumentException("No checkout attribute value found with the specified id");

            if (ModelState.IsValid)
            {
                checkoutAttribute.CheckoutAttributeValues.Remove(cav);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}
