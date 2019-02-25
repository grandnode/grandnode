using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.ExportImport;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.Messages;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Campaigns)]
    public partial class CampaignController : BaseAdminController
    {
        private readonly ICampaignService _campaignService;
        private readonly ICampaignViewModelService _campaignViewModelService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IExportManager _exportManager;

        public CampaignController(ICampaignService campaignService, ICampaignViewModelService campaignViewModelService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService, 
            IStoreContext storeContext,
            IStoreService storeService,
            IExportManager exportManager)
		{
            this._campaignService = campaignService;
            this._campaignViewModelService = campaignViewModelService;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._exportManager = exportManager;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var model = _campaignViewModelService.PrepareCampaignModels();
            var gridModel = new DataSourceResult
            {
                Data = model.campaignModels,
                Total = model.totalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult Customers(string campaignId, DataSourceRequest command)
        {
            var campaign = _campaignService.GetCampaignById(campaignId);
            var customers = _campaignService.CustomerSubscriptions(campaign, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers,
                Total = customers.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult History(string campaignId, DataSourceRequest command)
        {
            var campaign = _campaignService.GetCampaignById(campaignId);
            var history = _campaignService.GetCampaignHistory(campaign, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = history.Select(x => new
                {
                    Email = x.Email,
                    SentDate = x.CreatedDateUtc,
                }),
                Total = history.TotalCount
            };
            return Json(gridModel);
        }

        public IActionResult ExportCsv(string campaignId)
        {
            try
            {
                var campaign = _campaignService.GetCampaignById(campaignId);
                var customers = _campaignService.CustomerSubscriptions(campaign);
                string result = _exportManager.ExportNewsletterSubscribersToTxt(customers.Select(x => x.Email).ToList());

                string fileName = String.Format("newsletter_emails_campaign_{0}_{1}.txt", campaign.Name, CommonHelper.GenerateRandomDigitCode(4));
                return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        public IActionResult Create()
        {
            var model = _campaignViewModelService.PrepareCampaignModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var campaign = _campaignViewModelService.InsertCampaignModel(model);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            model = _campaignViewModelService.PrepareCampaignModel(model);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            var model = _campaignViewModelService.PrepareCampaignModel(campaign);
            return View(model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(CampaignModel model, bool continueEditing)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = _campaignViewModelService.UpdateCampaignModel(campaign, model);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Updated"));
                //selected tab
                SaveSelectedTabIndex();

                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }
            model = _campaignViewModelService.PrepareCampaignModel(model);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-test-email")]
        public IActionResult SendTestEmail(CampaignModel model)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            model = _campaignViewModelService.PrepareCampaignModel(model);
            try
            {
                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                if (emailAccount == null)
                    throw new GrandException("Email account could not be loaded");


                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(model.TestEmail, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    //there's a subscription. let's use it
                    var subscriptions = new List<NewsLetterSubscription>();
                    subscriptions.Add(subscription);
                    _campaignService.SendCampaign(campaign, emailAccount, subscriptions);
                }
                else
                {
                    //no subscription found
                    _campaignService.SendCampaign(campaign, emailAccount, model.TestEmail);
                }

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.TestEmailSentToCustomers"), false);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc, false);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-mass-email")]
        public IActionResult SendMassEmail(CampaignModel model)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            model = _campaignViewModelService.PrepareCampaignModel(model);
            model.CustomerTags = campaign.CustomerTags.ToList();
            model.CustomerRoles = campaign.CustomerRoles.ToList();
            try
            {
                var emailAccount = _emailAccountService.GetEmailAccountById(campaign.EmailAccountId);
                if (emailAccount == null)
                    throw new GrandException("Email account could not be loaded");

                //subscribers of certain store?
                var store = _storeService.GetStoreById(campaign.StoreId);
                var storeId = store != null ? store.Id : "";
                var subscriptions = _campaignService.CustomerSubscriptions(campaign);
                var totalEmailsSent = _campaignService.SendCampaign(campaign, emailAccount, subscriptions);
                SuccessNotification(string.Format(_localizationService.GetResource("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), totalEmailsSent), false);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc, false);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _campaignService.DeleteCampaign(campaign);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }
	}
}
