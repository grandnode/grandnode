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
using System.Threading.Tasks;

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
            _addressAttributeService = addressAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _addressAttributeViewModelService = addressAttributeViewModelService;
        }

        #endregion
        
        
        #region Address attributes

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult ListBlock() => PartialView("ListBlock");

        public async Task<IActionResult> List()
        {
            //select third tab
            const int addressFormFieldIndex = 2;
            await SaveSelectedTabIndex(addressFormFieldIndex);
            return RedirectToAction("CustomerUser", "Setting");
        }

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var model = await _addressAttributeViewModelService.PrepareAddressAttributes();
            var gridModel = new DataSourceResult
            {
                Data = model.addressAttributes,
                Total = model.totalCount
            };
            return Json(gridModel);
        }
        
        //create
        public async Task<IActionResult> Create()
        {
            var model = _addressAttributeViewModelService.PrepareAddressAttributeModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(AddressAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var addressAttribute = await _addressAttributeViewModelService.InsertAddressAttributeModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = addressAttribute.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public async Task<IActionResult> Edit(string id)
        {
            var addressAttribute = await _addressAttributeService.GetAddressAttributeById(id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeModel(addressAttribute);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = addressAttribute.GetLocalized(x => x.Name, languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(AddressAttributeModel model, bool continueEditing)
        {
            var addressAttribute = await _addressAttributeService.GetAddressAttributeById(model.Id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                addressAttribute = await _addressAttributeViewModelService.UpdateAddressAttributeModel(model, addressAttribute);
                SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = addressAttribute.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var addressAttribute = await _addressAttributeService.GetAddressAttributeById(id);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            await _addressAttributeService.DeleteAddressAttribute(addressAttribute);

            SuccessNotification(_localizationService.GetResource("Admin.Address.AddressAttributes.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Address attribute values

        //list
        [HttpPost]
        public async Task<IActionResult> ValueList(string addressAttributeId, DataSourceRequest command)
        {
            var model = await _addressAttributeViewModelService.PrepareAddressAttributeValues(addressAttributeId);
            var gridModel = new DataSourceResult
            {
                Data = model.addressAttributeValues,
                Total = model.totalCount
            };
            return Json(gridModel);
        }

        //create
        public async Task<IActionResult> ValueCreatePopup(string addressAttributeId)
        {
            var addressAttribute = await _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeValueModel(addressAttributeId);
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ValueCreatePopup(AddressAttributeValueModel model)
        {
            var addressAttribute = await _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            if (addressAttribute == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                await _addressAttributeViewModelService.InsertAddressAttributeValueModel(model);
                ViewBag.RefreshPage = true;
                return View(model);
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public async Task<IActionResult> ValueEditPopup(string id, string addressAttributeId)
        {
            var av = await _addressAttributeService.GetAddressAttributeById(addressAttributeId);
            if(av == null)
                //No address attribute found with the specified id
                return RedirectToAction("List");

            var cav = av.AddressAttributeValues.FirstOrDefault(x=>x.Id == id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            var model = _addressAttributeViewModelService.PrepareAddressAttributeValueModel(cav);

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ValueEditPopup(AddressAttributeValueModel model)
        {
            var av = await _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                //No address attribute value found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _addressAttributeViewModelService.UpdateAddressAttributeValueModel(model, cav);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //delete
        [HttpPost]
        public async Task<IActionResult> ValueDelete(AddressAttributeValueModel model)
        {
            var av = await _addressAttributeService.GetAddressAttributeById(model.AddressAttributeId);
            var cav = av.AddressAttributeValues.FirstOrDefault(x => x.Id == model.Id);
            if (cav == null)
                throw new ArgumentException("No address attribute value found with the specified id");
            await _addressAttributeService.DeleteAddressAttributeValue(cav);

            return new NullJsonResult();
        }
        #endregion
    }
}
