using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Core;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Services.Stores;

namespace Grand.Web.Areas.Admin.Controllers
{
	public partial class ContactFormController : BaseAdminController
	{
		private readonly IContactUsService _contactUsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly IEmailAccountService _emailAccountService;

        public ContactFormController(IContactUsService contactUsService,
            IDateTimeHelper dateTimeHelper, 
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreService storeService,
            IEmailAccountService emailAccountService)
		{
            this._contactUsService = contactUsService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._storeService = storeService;
            this._emailAccountService = emailAccountService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

		public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

            var model = new ContactFormListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
		}

		[HttpPost]
		public IActionResult ContactFormList(DataSourceRequest command, ContactFormListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageContactForm))
                return AccessDeniedView();

            DateTime? startDateValue = (model.SearchStartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.SearchEndDate == null) ? null 
                            :(DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            string vendorId = ""; 
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var contactform = _contactUsService.GetAllContactUs(
                fromUtc:startDateValue, 
                toUtc: endDateValue, 
                email: model.SearchEmail,
                storeId: model.StoreId,
                vendorId: vendorId,
                pageIndex: command.Page - 1, 
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = contactform.Select(x => {
                    var store = _storeService.GetStoreById(x.StoreId);
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Enquiry = "";
                    m.Email = m.FullName + " - " + m.Email;
                    m.Store = store != null ? store.Name : "-empty-";
                    return m;
                }),
                Total = contactform.TotalCount
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

            var model = contactform.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(contactform.CreatedOnUtc, DateTimeKind.Utc);
            var store = _storeService.GetStoreById(contactform.StoreId);
            model.Store = store!=null ? store.Name: "-empty-";
            var email = _emailAccountService.GetEmailAccountById(contactform.EmailAccountId);
            model.EmailAccountName = email != null ? email.DisplayName : "-empty-";
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
