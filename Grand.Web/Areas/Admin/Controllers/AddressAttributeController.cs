using System;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class AddressAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public AddressAttributeController(IAddressAttributeService addressAttributeService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPermissionService permissionService)
        {
            this._addressAttributeService = addressAttributeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._permissionService = permissionService;
        }

        #endregion
        
        
        #region Address attributes

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

            //we just redirect a user to the address settings page
            
            //select third tab
            const int addressFormFieldIndex = 2;
            SaveSelectedTabIndex(addressFormFieldIndex);
            return RedirectToAction("CustomerUser", "Setting");
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var addressAttributes = _addressAttributeService.GetAllAddressAttributes();
            var gridModel = new DataSourceResult
            {
                Data = addressAttributes.Select(x =>
                {
                    var attributeModel = x.ToModel();
                    attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                    return attributeModel;
                }),
                Total = addressAttributes.Count()
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = new AddressAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(AddressAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var addressAttribute = model.ToEntity();
                addressAttribute.Locales.Clear();
                foreach(var local in model.Locales)
                {
                    if(!(String.IsNullOrEmpty(local.Name)))
                        addressAttribute.Locales.Add(new Core.Domain.Localization.LocalizedProperty()
                        {
                            LanguageId = local.LanguageId,
                            LocaleKey = "Name",
                            LocaleValue = local.Name
                        });
                }
                _addressAttributeService.InsertAddressAttribute(addressAttribute);
                //locales
                //UpdateAttributeLocales(addressAttribute, model);


                SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = addressAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var addressAttribute = _addressAttributeService.GetAddressAttributeById(id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = addressAttribute.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = addressAttribute.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(AddressAttributeModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var addressAttribute = _addressAttributeService.GetAddressAttributeById(model.Id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                addressAttribute = model.ToEntity(addressAttribute);
                addressAttribute.Locales.Clear();
                foreach (var local in model.Locales)
                {
                    if (!(String.IsNullOrEmpty(local.Name)))
                        addressAttribute.Locales.Add(new Core.Domain.Localization.LocalizedProperty()
                        {
                            LanguageId = local.LanguageId,
                            LocaleKey = "Name",
                            LocaleValue = local.Name
                        });
                }
                _addressAttributeService.UpdateAddressAttribute(addressAttribute);
                //locales
                //UpdateAttributeLocales(addressAttribute, model);

                SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = addressAttribute.Id});
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

            var addressAttribute = _addressAttributeService.GetAddressAttributeById(id);
            _addressAttributeService.DeleteAddressAttribute(addressAttribute);

            SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Address attribute values

        //list
        [HttpPost]
        public IActionResult ValueList(string addressAttributeId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var values = _addressAttributeService.GetAddressAttributeById(addressAttributeId).AddressAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x => new AddressAttributeValueModel
                {
                    Id = x.Id,
                    AddressAttributeId = x.AddressAttributeId,
                    Name = x.Name,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                }),
                Total = values.Count()
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string addressAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var addressAttribute = _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = new AddressAttributeValueModel();
            model.AddressAttributeId = addressAttributeId;
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(AddressAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var addressAttribute = _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                var cav = new AddressAttributeValue
                {
                    AddressAttributeId = model.AddressAttributeId,
                    Name = model.Name,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };

                addressAttribute.Locales.Clear();
                foreach (var local in model.Locales)
                {
                    if(!(String.IsNullOrEmpty(local.Name)))
                        addressAttribute.Locales.Add(new Core.Domain.Localization.LocalizedProperty()
                        {
                            LanguageId = local.LanguageId,
                            LocaleKey = "Name",
                            LocaleValue = local.Name
                        });
                }

                _addressAttributeService.InsertAddressAttributeValue(cav);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string addressAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var av = _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x=>x.Id == id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            var model = new AddressAttributeValueModel
            {
                AddressAttributeId = cav.AddressAttributeId,
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
        public IActionResult ValueEditPopup(AddressAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var av = _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                cav.Name = model.Name;
                cav.IsPreSelected = model.IsPreSelected;
                cav.DisplayOrder = model.DisplayOrder;
                cav.Locales.Clear();
                foreach (var local in model.Locales)
                {
                    cav.Locales.Add(new Core.Domain.Localization.LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name
                    });
                }

                _addressAttributeService.UpdateAddressAttributeValue(cav);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ValueDelete(AddressAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var av = _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                throw new ArgumentException("No address attribute value found with the specified id");
            _addressAttributeService.DeleteAddressAttributeValue(cav);

            return new NullJsonResult();
        }


        #endregion
    }
}
