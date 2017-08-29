using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Services.Customers;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
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


        public IActionResult Index()
		{
			return RedirectToAction("List");
		}

		public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var model = new NewsLetterSubscriptionListModel();

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //active
            model.ActiveList.Add(new SelectListItem
            {
                Value = " ",
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

            foreach (var ca in _newsletterCategoryService.GetAllNewsletterCategory())
                model.AvailableCategories.Add(new SelectListItem { Text = ca.Name, Value = ca.Id.ToString() });

            return View(model);
		}

		[HttpPost]
		public IActionResult SubscriptionList(DataSourceRequest command, NewsLetterSubscriptionListModel model, string[] searchCategoryIds)
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
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc).ToLongTimeString();
                    m.Categories = GetCategoryNames(x.Categories.ToList());
                    return m;
				}),
                Total = newsletterSubscriptions.TotalCount
            };

            return Json(gridModel);
		}

        [HttpPost]
        public IActionResult SubscriptionUpdate(NewsLetterSubscriptionModel model)
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
        public IActionResult SubscriptionDelete(string id)
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
		public IActionResult ExportCsv(NewsLetterSubscriptionListModel model, string[] searchCategoryIds)
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
        public IActionResult ImportCsv(IFormFile importcsvfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            try
            {
                if (importcsvfile != null && importcsvfile.Length > 0)
                {
                    int count = _importManager.ImportNewsletterSubscribersFromTxt(importcsvfile.OpenReadStream());
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
