using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ContactFormController : BaseAdminController
	{
		private readonly IContactUsService _contactUsService;
        private readonly IContactFormViewModelService _contactFormViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public ContactFormController(IContactUsService contactUsService,
            IContactFormViewModelService contactFormViewModelService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
		{
            this._contactUsService = contactUsService;
            this._contactFormViewModelService = contactFormViewModelService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

		public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

            var model = _contactFormViewModelService.PrepareContactFormListModel();
            return View(model);
		}

		[HttpPost]
		public IActionResult ContactFormList(DataSourceRequest command, ContactFormListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

			var contactform = _contactUsService.GetContactUsById(id);
            if (contactform == null)
                return RedirectToAction("List");

            var model = _contactFormViewModelService.PrepareContactFormModel(contactform);
            return View(model);
		}

	    [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

            var contactform = _contactUsService.GetContactUsById(id);
            if (contactform == null)
                //No email found with the specified id
                return RedirectToAction("List");

            _contactUsService.DeleteContactUs(contactform);

            SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.Deleted"));
			return RedirectToAction("List");
		}

        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public IActionResult DeleteAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

            _contactUsService.ClearTable();

            SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.DeletedAll"));
            return RedirectToAction("List");
        }

	}
}
