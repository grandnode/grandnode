using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Messages;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ContactAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IContactAttributeService _contactAttributeService;
        private readonly IContactAttributeParser _contactAttributeParser;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;

        #endregion

        #region Constructors

        public ContactAttributeController(IContactAttributeService contactAttributeService,
            IContactAttributeParser contactAttributeParser,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IWorkContext workContext, 
            ICurrencyService currencyService, 
            ICustomerActivityService customerActivityService, 
            IMeasureService measureService, 
            IPermissionService permissionService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            IAclService aclService)
        {
            this._contactAttributeService = contactAttributeService;
            this._contactAttributeParser = contactAttributeParser;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
            this._aclService = aclService;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateAttributeLocales(ContactAttribute contactAttribute, ContactAttributeModel model)
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
        protected virtual List<LocalizedProperty> UpdateValueLocales(ContactAttributeValue contactAttributeValue, ContactAttributeValueModel model)
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
        protected virtual void PrepareStoresMappingModel(ContactAttributeModel model, ContactAttribute contactAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (contactAttribute != null)
                {
                    model.SelectedStoreIds = contactAttribute.Stores.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareAclModel(ContactAttributeModel model, ContactAttribute contactAttribute, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (contactAttribute != null)
                {
                    model.SelectedCustomerRoleIds = contactAttribute.CustomerRoles.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareConditionAttributes(ContactAttributeModel model, ContactAttribute contactAttribute)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //currenty any contact attribute can have condition.
            model.ConditionAllowed = true;

            if (contactAttribute == null)
                return;

            var selectedAttribute = _contactAttributeParser.ParseContactAttributes(contactAttribute.ConditionAttributeXml).FirstOrDefault();
            var selectedValues = _contactAttributeParser.ParseContactAttributeValues(contactAttribute.ConditionAttributeXml);

            model.ConditionModel = new ConditionModel()
            {
                EnableCondition = !string.IsNullOrEmpty(contactAttribute.ConditionAttributeXml),
                SelectedAttributeId = selectedAttribute != null ? selectedAttribute.Id : "",
                ConditionAttributes = _contactAttributeService.GetAllContactAttributes()
                    //ignore this attribute and non-combinable attributes
                    .Where(x => x.Id != contactAttribute.Id && x.CanBeUsedAsCondition())
                    .Select(x =>
                        new AttributeConditionModel()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AttributeControlType = x.AttributeControlType,
                            Values = x.ContactAttributeValues 
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
        protected virtual void SaveConditionAttributes(ContactAttribute contactAttribute, ContactAttributeModel model)
        {
            string attributesXml = null;
            if (model.ConditionModel.EnableCondition)
            {
                var attribute = _contactAttributeService.GetContactAttributeById(model.ConditionModel.SelectedAttributeId);
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
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, selectedValue);
                                else
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValues = selectedAttribute != null ? selectedAttribute.Values.Where(x => x.Selected).Select(x => x.Value) : null;
                                if (selectedValues.Any())
                                    foreach (var value in selectedValues)
                                        attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, value);
                                else
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, string.Empty);
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
            contactAttribute.ConditionAttributeXml = attributesXml;
        }


        #endregion

        #region Contact attributes

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

            var contactAttributes = _contactAttributeService.GetAllContactAttributes();
            var gridModel = new DataSourceResult
            {
                Data = contactAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                }),
                Total = contactAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var model = new ContactAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //ACL
            PrepareAclModel(model, null, false);
            //condition
            PrepareConditionAttributes(model, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ContactAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var contactAttribute = model.ToEntity();
                contactAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                contactAttribute.Locales = UpdateAttributeLocales(contactAttribute, model);
                contactAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _contactAttributeService.InsertContactAttribute(contactAttribute);
               
                //activity log
                _customerActivityService.InsertActivity("AddNewContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewContactAttribute"), contactAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ContactAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = contactAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

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

            var contactAttribute = _contactAttributeService.GetContactAttributeById(id);
            if (contactAttribute == null)
                //No contact attribute found with the specified id
                return RedirectToAction("List");

            var model = contactAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = contactAttribute.GetLocalized(x => x.Name, languageId, false, false);
                locale.TextPrompt = contactAttribute.GetLocalized(x => x.TextPrompt, languageId, false, false);
            });
            //ACL
            PrepareAclModel(model, contactAttribute, false);
            //Stores
            PrepareStoresMappingModel(model, contactAttribute, false);
            //condition
            PrepareConditionAttributes(model, contactAttribute);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ContactAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(model.Id);
            if (contactAttribute == null)
                //No contact attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                contactAttribute = model.ToEntity(contactAttribute);
                SaveConditionAttributes(contactAttribute, model);
                contactAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                contactAttribute.Locales = UpdateAttributeLocales(contactAttribute, model);
                contactAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _contactAttributeService.UpdateContactAttribute(contactAttribute);
               
                //activity log
                _customerActivityService.InsertActivity("EditContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.EditContactAttribute"), contactAttribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ContactAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = contactAttribute.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //Stores
            PrepareStoresMappingModel(model, contactAttribute, true);
            //ACL
            PrepareAclModel(model, contactAttribute, false);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(id);
            _contactAttributeService.DeleteContactAttribute(contactAttribute);

            //activity log
            _customerActivityService.InsertActivity("DeleteContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.DeleteContactAttribute"), contactAttribute.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ContactAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Contact attribute values

        //list
        [HttpPost]
        public IActionResult ValueList(string contactAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
            var contactAttribute = _contactAttributeService.GetContactAttributeById(contactAttributeId);
            var values = contactAttribute.ContactAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x => new ContactAttributeValueModel
                {
                    Id = x.Id,
                    ContactAttributeId = x.ContactAttributeId,
                    Name = contactAttribute.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                    ColorSquaresRgb = x.ColorSquaresRgb,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                }),
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string contactAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(contactAttributeId);
            var model = new ContactAttributeValueModel();
            model.ContactAttributeId = contactAttributeId;

            //color squares
            model.DisplayColorSquaresRgb = contactAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(ContactAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(model.ContactAttributeId);
            if (contactAttribute == null)
                //No contact attribute found with the specified id
                return RedirectToAction("List");

            if (contactAttribute.AttributeControlType == AttributeControlType.ColorSquares)
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
                var cav = new ContactAttributeValue
                {
                    ContactAttributeId = model.ContactAttributeId,
                    Name = model.Name,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                };
                cav.Locales = UpdateValueLocales(cav, model);
                contactAttribute.ContactAttributeValues.Add(cav);
                _contactAttributeService.UpdateContactAttribute(contactAttribute);
                
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string contactAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
            var contactAttribute = _contactAttributeService.GetContactAttributeById(contactAttributeId);
            var cav = contactAttribute.ContactAttributeValues.Where(x=>x.Id == id).FirstOrDefault();
            if (cav == null)
                //No contact attribute value found with the specified id
                return RedirectToAction("List");

            var model = new ContactAttributeValueModel
            {
                ContactAttributeId = cav.ContactAttributeId,
                Name = cav.Name,
                ColorSquaresRgb = cav.ColorSquaresRgb,
                DisplayColorSquaresRgb = contactAttribute.AttributeControlType == AttributeControlType.ColorSquares,
                IsPreSelected = cav.IsPreSelected,
                DisplayOrder = cav.DisplayOrder,
            };

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult ValueEditPopup(ContactAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(model.ContactAttributeId);

            var cav = contactAttribute.ContactAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (cav == null)
                //No contact attribute value found with the specified id
                return RedirectToAction("List");

            if (contactAttribute.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            if (ModelState.IsValid)
            {
                cav.Name = model.Name;
                cav.ColorSquaresRgb = model.ColorSquaresRgb;
                cav.IsPreSelected = model.IsPreSelected;
                cav.DisplayOrder = model.DisplayOrder;
                cav.Locales = UpdateValueLocales(cav, model);
                _contactAttributeService.UpdateContactAttribute(contactAttribute);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ValueDelete(string id, string contactAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var contactAttribute = _contactAttributeService.GetContactAttributeById(contactAttributeId);
            var cav = contactAttribute.ContactAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (cav == null)
                throw new ArgumentException("No contact attribute value found with the specified id");

            contactAttribute.ContactAttributeValues.Remove(cav);
            _contactAttributeService.UpdateContactAttribute(contactAttribute);

            return new NullJsonResult();
        }


        #endregion
    }
}
