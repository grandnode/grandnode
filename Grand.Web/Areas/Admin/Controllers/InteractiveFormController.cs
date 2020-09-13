using Grand.Domain.Messages;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.InteractiveForms)]
    public partial class InteractiveFormController : BaseAdminController
    {
        private readonly IInteractiveFormService _interactiveFormService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEmailAccountService _emailAccountService;

        public InteractiveFormController(IInteractiveFormService interactiveFormService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
            IEmailAccountService emailAccountService)
        {
            _interactiveFormService = interactiveFormService;
            _localizationService = localizationService;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
            _emailAccountService = emailAccountService;
        }

        #region Utilities

        private string FormatTokens(string[] tokens)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                sb.Append(token);
                if (i != tokens.Length - 1)
                    sb.Append(", ");
            }
            sb.Append(", %sendbutton%");
            sb.Append(", %errormessage%");
            return sb.ToString();
        }
        #endregion

        #region List forms

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var forms = await _interactiveFormService.GetAllForms();
            var gridModel = new DataSourceResult
            {
                Data = forms.Select(x =>
                {
                    var model = x.ToModel();
                    model.Body = "";
                    return model;
                }),
                Total = forms.Count
            };
            return Json(gridModel);
        }
        #endregion

        #region Update form

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new InteractiveFormModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(InteractiveFormModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var form = model.ToEntity();
                form.CreatedOnUtc = DateTime.UtcNow;

                await _interactiveFormService.InsertForm(form);
                await _customerActivityService.InsertActivity("InteractiveFormAdd", form.Id, _localizationService.GetResource("ActivityLog.InteractiveFormAdd"), form.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.InteractiveForms.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = form.Id }) : RedirectToAction("List");
            }
            //locales
            await AddLocales(_languageService, model.Locales);
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var form = await _interactiveFormService.GetFormById(id);
            if (form == null)
                return RedirectToAction("List");

            var model = form.ToModel();

            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = form.GetLocalized(x => x.Name, languageId, false, false);
                locale.Body = form.GetLocalized(x => x.Body, languageId, false, false);
            });
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());

            //available tokens
            model.AvailableTokens = FormatTokens(form.FormAttributes.Select(x => "%" + x.SystemName + "%").ToArray());
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(InteractiveFormModel model, bool continueEditing)
        {
            var form = await _interactiveFormService.GetFormById(model.Id);
            if (form == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                form = model.ToEntity(form);
                await _interactiveFormService.UpdateForm(form);

                await _customerActivityService.InsertActivity("InteractiveFormEdit", form.Id, _localizationService.GetResource("ActivityLog.InteractiveFormUpdate"), form.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.InteractiveForms.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = form.Id }) : RedirectToAction("List");
            }

            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = form.GetLocalized(x => x.Name, languageId, false, false);
                locale.Body = form.GetLocalized(x => x.Body, languageId, false, false);
            });
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());

            //available tokens
            model.AvailableTokens = FormatTokens(form.FormAttributes.Select(x => "%" + x.SystemName + "%").ToArray());

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var form = await _interactiveFormService.GetFormById(id);
            if (form == null)
                return RedirectToAction("List");

            await _interactiveFormService.DeleteForm(form);
            await _customerActivityService.InsertActivity("InteractiveFormDelete", form.Id, _localizationService.GetResource("ActivityLog.InteractiveFormDeleted"), form.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.InteractiveForms.Deleted"));
            return RedirectToAction("List");
        }
        #endregion

        #region Form attributes

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> FormAttributesList(string formId)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");

            var gridModel = new DataSourceResult
            {
                Data = form.FormAttributes.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    SystemName = x.SystemName,
                    Type = ((FormControlType)x.FormControlTypeId).ToString()
                }),
                Total = form.FormAttributes.Count()
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> FormAttributesDelete(string id, string formId)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");

            form.FormAttributes.Remove(form.FormAttributes.FirstOrDefault(x => x.Id == id));
            await _interactiveFormService.UpdateForm(form);
            await _customerActivityService.InsertActivity("InteractiveFormEdit", form.Id, _localizationService.GetResource("ActivityLog.InteractiveFormDeleteAttribute"), form.Name);

            return new NullJsonResult();
        }

        #region Form Attributes

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> AddAttribute(string formId)
        {
            InteractiveFormAttributeModel model = new InteractiveFormAttributeModel
            {
                FormId = formId
            };

            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> AddAttribute(InteractiveFormAttributeModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var form = await _interactiveFormService.GetFormById(model.FormId);
                if (form == null)
                {
                    return RedirectToAction("List");
                }
                var attribute = model.ToEntity();
                form.FormAttributes.Add(attribute);
                await _interactiveFormService.UpdateForm(form);

                await _customerActivityService.InsertActivity("InteractiveFormEdit", attribute.Id, _localizationService.GetResource("ActivityLog.InteractiveFormAddAttribute"), attribute.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Added"));

                return continueEditing ? RedirectToAction("EditAttribute", new { formId = form.Id, aid = attribute.Id }) : RedirectToAction("Edit", new { id = form.Id });
            }
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> EditAttribute(string formId, string aid)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");

            var attribute = form.FormAttributes.FirstOrDefault(x => x.Id == aid);
            if (attribute == null)
                return RedirectToAction("List");

            var model = attribute.ToModel();
            model.FormId = formId;

            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = attribute.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditAttribute(string formId, InteractiveFormAttributeModel model, bool continueEditing)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");

            var attribute = form.FormAttributes.FirstOrDefault(x => x.Id == model.Id);
            if (attribute == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    attribute = model.ToEntity(attribute);
                    await _interactiveFormService.UpdateForm(form);
                    await _customerActivityService.InsertActivity("InteractiveFormEdit", attribute.Id, _localizationService.GetResource("ActivityLog.InteractiveFormUpdateAttribute"), attribute.Name);
                    SuccessNotification(_localizationService.GetResource("Admin.Promotions.InteractiveForms.Attribute.Updated"));
                    return continueEditing ? RedirectToAction("EditAttribute", new { formId = form.Id, aid = attribute.Id }) : RedirectToAction("Edit", new { id = form.Id });
                }
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = form.Id });
            }
        }

        #endregion

        #region Attribute Value

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AttributeValuesList(string formId, string aId)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");
            var values = form.FormAttributes.FirstOrDefault(x => x.Id == aId).FormAttributeValues;
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                }).OrderBy(x=>x.DisplayOrder),
                Total = values.Count()
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ValueCreatePopup(string form, string aId, string btnId, string formId)
        {
            var fo = await _interactiveFormService.GetFormById(form);
            if (fo == null)
                return RedirectToAction("List");
            var attribute = fo.FormAttributes.FirstOrDefault(x => x.Id == aId);
            if (attribute == null)
                return RedirectToAction("List");

            var model = new InteractiveFormAttributeValueModel
            {
                FormId = fo.Id,
                AttributeId = aId
            };

            ViewBag.RefreshPage = false;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            await AddLocales(_languageService, model.Locales);
            return View(model);
        }
        
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ValueCreatePopup(string btnId, string formId, string form, string aId, InteractiveFormAttributeValueModel model)
        {
            var fo = await _interactiveFormService.GetFormById(form);
            if (fo == null)
                return RedirectToAction("List");
            var attribute = fo.FormAttributes.FirstOrDefault(x => x.Id == aId);
            if (attribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var vaf = model.ToEntity();
                attribute.FormAttributeValues.Add(vaf);
                await _interactiveFormService.UpdateForm(fo);
                await _customerActivityService.InsertActivity("InteractiveFormEdit", vaf.Id, _localizationService.GetResource("ActivityLog.InteractiveFormAddAttributeValue"), vaf.Name);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        //edit
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ValueEditPopup(string id, string form, string aId, string formId, string btnId)
        {
            var fo = await _interactiveFormService.GetFormById(form);
            if (fo == null)
                return RedirectToAction("List");
            var attribute = fo.FormAttributes.FirstOrDefault(x => x.Id == aId);
            if (attribute == null)
                return RedirectToAction("List");

            var vaf = attribute.FormAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (vaf == null)
                return RedirectToAction("List");

            var model = vaf.ToModel();
            model.FormId = fo.Id;
            model.AttributeId = aId;
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = vaf.GetLocalized(x => x.Name, languageId, false, false);
            });
            ViewBag.RefreshPage = false;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ValueEditPopup(string btnId, string formId, string form, string aId, InteractiveFormAttributeValueModel model)
        {
            var fo = await _interactiveFormService.GetFormById(form);
            if (fo == null)
                return RedirectToAction("List");
            var attribute = fo.FormAttributes.FirstOrDefault(x => x.Id == aId);
            if (attribute == null)
                return RedirectToAction("List");

            var vaf = attribute.FormAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (vaf == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                vaf = model.ToEntity();
                await _interactiveFormService.UpdateForm(fo);
                await _customerActivityService.InsertActivity("InteractiveFormEdit", vaf.Id, _localizationService.GetResource("ActivityLog.InteractiveFormUpdateAttributeValue"), vaf.Name);
                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> AttributeValuesDelete(string id, string formId, string aId)
        {
            var form = await _interactiveFormService.GetFormById(formId);
            if (form == null)
                return RedirectToAction("List");
            var attribute = form.FormAttributes.FirstOrDefault(x => x.Id == aId);
            if (attribute == null)
                return RedirectToAction("List");

            var vaf = attribute.FormAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (vaf == null)
                return RedirectToAction("List");

            attribute.FormAttributeValues.Remove(vaf);
            await _interactiveFormService.UpdateForm(form);
            await _customerActivityService.InsertActivity("InteractiveFormEdit", vaf.Id, _localizationService.GetResource("ActivityLog.InteractiveFormDeleteAttributeValue"), vaf.Name);
            return new NullJsonResult();
        }

        #endregion


        #endregion

    }
}
