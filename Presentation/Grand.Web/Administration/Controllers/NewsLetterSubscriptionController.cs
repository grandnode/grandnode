using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Messages;
using Grand.Core;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Admin.Controllers
{
	public partial class NewsLetterSubscriptionController : BaseAdminController
	{
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;

		public NewsLetterSubscriptionController(INewsLetterSubscriptionService newsLetterSubscriptionService,
            INewsletterCategoryService newsletterCategoryService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreService storeService,
            ICustomerService customerService,
            IExportManager exportManager,
            IImportManager importManager)
		{
			this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._newsletterCategoryService = newsletterCategoryService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._customerService = customerService;
            this._exportManager = exportManager;
            this._importManager = importManager;
		}

        [NonAction]
        protected virtual string GetCategoryNames(IList<string> categoryNames, string separator = ",")
        {
            var sb = new StringBuilder();
            for (int i = 0; i < categoryNames.Count; i++)
            {
                var category = _newsletterCategoryService.GetNewsletterCategoryById(categoryNames[i]);
                if (category != null)
                {
                    sb.Append(category.Name);
                    if (i != categoryNames.Count - 1)
                    {
                        sb.Append(separator);
                        sb.Append(" ");
                    }
                }
            }
            return sb.ToString();
        }


        public ActionResult Index()
		{
			return RedirectToAction("List");
		}

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var model = new NewsLetterSubscriptionListModel();

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //active
            model.ActiveList.Add(new SelectListItem
            {
                Value = "",
                Text = _localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.List.SearchActive.All")
            });
            model.ActiveList.Add(new SelectListItem
            {
                Value = "1",
                Text = _localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.List.SearchActive.ActiveOnly")
            });
            model.ActiveList.Add(new SelectListItem
            {
                Value = "2",
                Text = _localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.List.SearchActive.NotActiveOnly")
            });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var cr in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = cr.Name, Value = cr.Id.ToString() });

            foreach (var ca in _newsletterCategoryService.GetAllNewsletterCategory())
                model.AvailableCategories.Add(new SelectListItem { Text = ca.Name, Value = ca.Id.ToString() });

            return View(model);
		}

		[HttpPost]
		public ActionResult SubscriptionList(DataSourceRequest command, NewsLetterSubscriptionListModel model, string[] searchCategoryIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            bool? isActive = null;
            if (model.ActiveId == 1)
                isActive = true;
            else if (model.ActiveId == 2)
                isActive = false;

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(model.SearchEmail,
                model.StoreId, isActive, searchCategoryIds, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = newsletterSubscriptions.Select(x =>
				{
					var m = x.ToModel();
				    var store = _storeService.GetStoreById(x.StoreId);
				    m.StoreName = store != null ? store.Name : "Unknown store";
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Categories = GetCategoryNames(x.Categories.ToList());
                    return m;
				}),
                Total = newsletterSubscriptions.TotalCount
            };

            return Json(gridModel);
		}

        [HttpPost]
        public ActionResult SubscriptionUpdate([Bind(Exclude = "CreatedOn")] NewsLetterSubscriptionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(model.Id);
            subscription.Email = model.Email;
            subscription.Active = model.Active;
            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult SubscriptionDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);
            if (subscription == null)
                throw new ArgumentException("No subscription found with the specified id");
            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            return new NullJsonResult();
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportcsv")]
		public ActionResult ExportCsv(NewsLetterSubscriptionListModel model, string[] searchCategoryIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            bool? isActive = null;
            if (model.ActiveId == 1)
                isActive = true;
            else if (model.ActiveId == 2)
                isActive = false;

			var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(model.SearchEmail,
                model.StoreId, isActive, searchCategoryIds);

		    string result = _exportManager.ExportNewsletterSubscribersToTxt(subscriptions);

            string fileName = String.Format("newsletter_emails_{0}_{1}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));
			return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
		}

        [HttpPost]
        public ActionResult ImportCsv(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            try
            {
                var file = Request.Files["importcsvfile"];
                if (file != null && file.ContentLength > 0)
                {
                    int count = _importManager.ImportNewsletterSubscribersFromTxt(file.InputStream);
                    SuccessNotification(String.Format(_localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess"), count));
                    return RedirectToAction("List");
                }
                ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }
	}
}
