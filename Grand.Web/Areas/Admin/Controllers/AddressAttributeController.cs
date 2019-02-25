using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Settings)]
    public partial class AddressAttributeController : BaseAdminController
    {
        #region Fields

        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressAttributeViewModelService _addressAttributeViewModelService;
        #endregion

        #region Constructors

        public AddressAttributeController(IAddressAttributeService addressAttributeService,
            ILanguageService languageService, 
            ILocalizationService localizationService,
            IAddressAttributeViewModelService addressAttributeViewModelService)
        {
            this._addressAttributeService = addressAttributeService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._addressAttributeViewModelService = addressAttributeViewModelService;
        }

        #endregion
        
        
        #region Address attributes

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult ListBlock() => PartialView("ListBlock");

        public IActionResult List()
        {
            //select third tab
            const int addressFormFieldIndex = 2;
            SaveSelectedTabIndex(addressFormFieldIndex);
            return RedirectToAction("CustomerUser", "Setting");
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var model = _addressAttributeViewModelService.PrepareAddressAttributes();
            var gridModel = new DataSourceResult
            {
                Data = model.addressAttributes,
                Total = model.totalCount
            };
            return Json(gridModel);
        }
        
        //create
        public IActionResult Create()
        {
            var model = _addressAttributeViewModelService.PrepareAddressAttributeModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(AddressAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var addressAttribute = _addressAttributeViewModelService.InsertAddressAttributeModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = addressAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult Edit(string id)
        {
            var addressAttribute = _addressAttributeService.GetAddressAttributeById(id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeModel(addressAttribute);
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
            var addressAttribute = _addressAttributeService.GetAddressAttributeById(model.Id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                addressAttribute = _addressAttributeViewModelService.UpdateAddressAttributeModel(model, addressAttribute);
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
            var addressAttribute = _addressAttributeService.GetAddressAttributeById(id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

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
            var model = _addressAttributeViewModelService.PrepareAddressAttributeValues(addressAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = model.addressAttributeValues,
                Total = model.totalCount
            };
            return Json(gridModel);
        }

        //create
        public IActionResult ValueCreatePopup(string addressAttributeId)
        {
            var addressAttribute = _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeValueModel(addressAttributeId);
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public IActionResult ValueCreatePopup(AddressAttributeValueModel model)
        {
            var addressAttribute = _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                _addressAttributeViewModelService.InsertAddressAttributeValueModel(model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public IActionResult ValueEditPopup(string id, string addressAttributeId)
        {
            var av = _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            if(av == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var cav = av.AddressAttributeValues.FirstOrDefault(x=>x.Id == id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeValueModel(cav);

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
            var av = _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _addressAttributeViewModelService.UpdateAddressAttributeValueModel(model, cav);
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
