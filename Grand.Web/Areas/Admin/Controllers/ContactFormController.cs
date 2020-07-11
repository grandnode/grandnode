using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            _contactUsService = contactUsService;
            _contactFormViewModelService = contactFormViewModelService;
            _localizationService = localizationService;
        }

        public IActionResult Index() => RedirectToAction("List");

		public async Task<IActionResult> List()
        {
            var model = await _contactFormViewModelService.PrepareContactFormListModel();
            return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ContactFormList(DataSourceRequest command, ContactFormListModel model)
        {
            var (contactFormModel, totalCount) = await _contactFormViewModelService.PrepareContactFormListModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = contactFormModel,
                Total = totalCount
            };
            return Json(gridModel);
        }

		public async Task<IActionResult> Details(string id)
        {
			var contactform = await _contactUsService.GetContactUsById(id);
            if (contactform == null)
                return RedirectToAction("List");

            var model = await _contactFormViewModelService.PrepareContactFormModel(contactform);
            return View(model);
		}

	    [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var contactform = await _contactUsService.GetContactUsById(id);
            if (contactform == null)
                //No email found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                await _contactUsService.DeleteContactUs(contactform);

                SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            await _contactUsService.ClearTable();

            SuccessNotification(_localizationService.GetResource("Admin.System.ContactForm.DeletedAll"));
            return RedirectToAction("List");
        }

	}
}
