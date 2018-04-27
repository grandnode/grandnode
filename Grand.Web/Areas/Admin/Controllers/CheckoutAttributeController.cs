using System;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Orders;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Core.Domain.Localization;
using System.Collections.Generic;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.Filters;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CheckoutAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;

        #endregion

        #region Constructors

        public CheckoutAttributeController(ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            ITaxCategoryService taxCategoryService,
            IWorkContext workContext, 
            ICurrencyService currencyService, 
            ICustomerActivityService customerActivityService, 
            CurrencySettings currencySettings,
            IMeasureService measureService, 
            MeasureSettings measureSettings,
            IPermissionService permissionService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            IAclService aclService)
        {
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._taxCategoryService = taxCategoryService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._customerActivityService = customerActivityService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
            this._aclService = aclService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateAttributeLocales(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,

                    });

                if (!(String.IsNullOrEmpty(local.TextPrompt)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "TextPrompt",
                        LocaleValue = local.TextPrompt,
                    });

            }
            return localized;
            

        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateValueLocales(CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });
            }
            return localized;

          
        }

        [NonAction]
        protected virtual void PrepareTaxCategories(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = checkoutAttribute != null && !excludeProperties && tc.Id == checkoutAttribute.TaxCategoryId });
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (checkoutAttribute != null)
                {
                    model.SelectedStoreIds = checkoutAttribute.Stores.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareAclModel(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (checkoutAttribute != null)
                {
                    model.SelectedCustomerRoleIds = checkoutAttribute.CustomerRoles.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareConditionAttributes(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //currenty any checkout attribute can have condition.
            model.ConditionAllowed = true;

            if (checkoutAttribute == null)
                return;

            var selectedAttribute = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttribute.ConditionAttributeXml).FirstOrDefault();
            var selectedValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttribute.ConditionAttributeXml);

            model.ConditionModel = new ConditionModel()
            {
                EnableCondition = !string.IsNullOrEmpty(checkoutAttribute.ConditionAttributeXml),
                SelectedAttributeId = selectedAttribute != null ? selectedAttribute.Id : "",
                ConditionAttributes = _checkoutAttributeService.GetAllCheckoutAttributes()
                    //ignore this attribute and non-combinable attributes
                    .Where(x => x.Id != checkoutAttribute.Id && x.CanBeUsedAsCondition())
                    .Select(x =>
                        new AttributeConditionModel()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AttributeControlType = x.AttributeControlType,
                            Values = x.CheckoutAttributeValues //_checkoutAttributeService.GetCheckoutAttributeValues(x.Id)
                                .Select(v => new SelectListItem()
                                {
                                    Text = v.Name,
                                    Value = v.Id.ToString(),
                                    Selected = selectedAttribute != null && selectedAttribute.Id == x.Id && selectedValues.Any(sv => sv.Id == v.Id)
                                }).ToList()
                        }).ToList()
            };
        }

        [NonAction]
        protected virtual void SaveConditionAttributes(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model)
        {
            string attributesXml = null;
            if (model.ConditionModel.EnableCondition)
            {
                var attribute = _checkoutAttributeService.GetCheckoutAttributeById(model.ConditionModel.SelectedAttributeId);
                if (attribute != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValue = selectedAttribute != null ? selectedAttribute.SelectedValueId : null;
                                if (!String.IsNullOrEmpty(selectedValue))
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, selectedValue);
                                else
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValues = selectedAttribute != null ? selectedAttribute.Values.Where(x => x.Selected).Select(x => x.Value) : null;
                                if (selectedValues.Any())
                                    foreach (var value in selectedValues)
                                        attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, value);
                                else
                                    attributesXml = _checkoutAttributeParser.AddCheckoutAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are not supported as conditions
                            break;
                    }
                }
            }
            checkoutAttribute.ConditionAttributeXml = attributesXml;
        }


        #endregion

        #region Checkout attributes

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes();
            var gridModel = new DataSourceResult
            {
                Data = checkoutAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                }),
                Total = checkoutAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var model = new CheckoutAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //tax categories
            PrepareTaxCategories(model, null, true);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //ACL
            PrepareAclModel(model, null, false);
            //condition
            PrepareConditionAttributes(model, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var checkoutAttribute = model.ToEntity();
                checkoutAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                checkoutAttribute.Locales = UpdateAttributeLocales(checkoutAttribute, model);
                checkoutAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _checkoutAttributeService.InsertCheckoutAttribute(checkoutAttribute);
               
                //activity log
                _customerActivityService.InsertActivity("AddNewCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewCheckoutAttribute"), checkoutAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = checkoutAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //tax categories
            PrepareTaxCategories(model, null, true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);

            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

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
            //ACL
            PrepareAclModel(model, checkoutAttribute, false);
            //tax categories
            PrepareTaxCategories(model, checkoutAttribute, false);
            //Stores
            PrepareStoresMappingModel(model, checkoutAttribute, false);
            //condition
            PrepareConditionAttributes(model, checkoutAttribute);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CheckoutAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(model.Id);
            if (checkoutAttribute == null)
                //No checkout attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                checkoutAttribute = model.ToEntity(checkoutAttribute);
                SaveConditionAttributes(checkoutAttribute, model);
                checkoutAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                checkoutAttribute.Locales = UpdateAttributeLocales(checkoutAttribute, model);
                checkoutAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);
               
                //activity log
                _customerActivityService.InsertActivity("EditCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.EditCheckoutAttribute"), checkoutAttribute.Name);

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
            PrepareTaxCategories(model, checkoutAttribute, true);
            //Stores
            PrepareStoresMappingModel(model, checkoutAttribute, true);
            //ACL
            PrepareAclModel(model, checkoutAttribute, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(id);
            _checkoutAttributeService.DeleteCheckoutAttribute(checkoutAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteCheckoutAttribute", checkoutAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteCheckoutAttribute"), checkoutAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Checkout attribute values

        //list
        [HttpPost]
        public IActionResult ValueList(string checkoutAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var values = checkoutAttribute.CheckoutAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x => new CheckoutAttributeValueModel
                {
                    Id = x.Id,
                    CheckoutAttributeId = x.CheckoutAttributeId,
                    Name = checkoutAttribute.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                    ColorSquaresRgb = x.ColorSquaresRgb,
                    PriceAdjustment = x.PriceAdjustment,
                    WeightAdjustment = x.WeightAdjustment,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                }),
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string checkoutAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var model = new CheckoutAttributeValueModel();
            model.CheckoutAttributeId = checkoutAttributeId;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;

            //color squares
            model.DisplayColorSquaresRgb = checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(CheckoutAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

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
                //TO DO
                //try
                //{
                //    //ensure color is valid (can be instanciated)
                //    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                //}
                //catch (Exception exc)
                //{
                //    ModelState.AddModelError("", exc.Message);
                //}
            }

            if (ModelState.IsValid)
            {
                var cav = new CheckoutAttributeValue
                {
                    CheckoutAttributeId = model.CheckoutAttributeId,
                    Name = model.Name,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                };
                cav.Locales = UpdateValueLocales(cav, model);
                checkoutAttribute.CheckoutAttributeValues.Add(cav);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);
                
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string checkoutAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var cav = checkoutAttribute.CheckoutAttributeValues.Where(x=>x.Id == id).FirstOrDefault();
            if (cav == null)
                //No checkout attribute value found with the specified id
                return RedirectToAction("List");

            var model = new CheckoutAttributeValueModel
            {
                CheckoutAttributeId = cav.CheckoutAttributeId,
                Name = cav.Name,
                ColorSquaresRgb = cav.ColorSquaresRgb,
                DisplayColorSquaresRgb = checkoutAttribute.AttributeControlType == AttributeControlType.ColorSquares,
                PriceAdjustment = cav.PriceAdjustment,
                WeightAdjustment = cav.WeightAdjustment,
                IsPreSelected = cav.IsPreSelected,
                DisplayOrder = cav.DisplayOrder,
                PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name
            };

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

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
                cav.Name = model.Name;
                cav.ColorSquaresRgb = model.ColorSquaresRgb;
                cav.PriceAdjustment = model.PriceAdjustment;
                cav.WeightAdjustment = model.WeightAdjustment;
                cav.IsPreSelected = model.IsPreSelected;
                cav.DisplayOrder = model.DisplayOrder;
                cav.Locales = UpdateValueLocales(cav, model);
                _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var checkoutAttribute = _checkoutAttributeService.GetCheckoutAttributeById(checkoutAttributeId);
            var cav = checkoutAttribute.CheckoutAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (cav == null)
                throw new ArgumentException("No checkout attribute value found with the specified id");

            checkoutAttribute.CheckoutAttributeValues.Remove(cav);
            _checkoutAttributeService.UpdateCheckoutAttribute(checkoutAttribute);

            return new NullJsonResult();
        }


        #endregion
    }
}
