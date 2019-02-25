using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.MessageContactForm)]
    public partial class ContactFormController : BaseAdminController
	{
		private readonly IContactUsService _contactUsService;
        private readonly IContactFormViewModelService _contactFormViewModelService;
        private readonly ILocalizationService _localizationService;

        public ContactFormController(IContactUsService contactUsService,
            IContactFormViewModelService contactFormViewModelService,
            ILocalizationService localizationService)
		{
            this._contactUsService = contactUsService;
            this._contactFormViewModelService = contactFormViewModelService;
            this._localizationService = localizationService;
        }

        public IActionResult Index() => RedirectToAction("List");

		public IActionResult List()
        {
            var model = _contactFormViewModelService.PrepareContactFormListModel();
            return View(model);
		}

		[HttpPost]
		public IActionResult ContactFormList(DataSourceRequest command, ContactFormListModel model)
        {
            var contactform = _contactFormViewModelService.PrepareContactFormListModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = contactform.contactFormModel,
                Total = contactform.totalCount
            };
            return Json(gridModel);
        }

		public IActionResult Details(string id)
        {
			var contactform = _contactUsService.GetContactUsById(id);
            if (contactform == null)
                return RedirectToAction("List");

            var model = _contactFormViewModelService.PrepareContactFormModel(contactform);
            return View(model);
		}

	    [HttpPost]
        public IActionResult Delete(string id)
        {
            var contactform = _contactUsService.GetContactUsById(id);
            if (contactform == null)
                //No email found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                _contactUsService.DeleteContactUs(contactform);

                SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public IActionResult DeleteAll()
        {
            _contactUsService.ClearTable();

            SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.DeletedAll"));
            return RedirectToAction("List");
        }

	}
}
