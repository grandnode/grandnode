﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Messages;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Services.Customers;
using Grand.Services.ExportImport;

namespace Grand.Admin.Controllers
{
	public partial class CampaignController : BaseAdminController
	{
        private readonly ICampaignService _campaignService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerTagService _customerTagService;
        private readonly IExportManager _exportManager;

        public CampaignController(ICampaignService campaignService,
            IDateTimeHelper dateTimeHelper, 
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService, 
            IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext,
            IStoreService storeService,
            IPermissionService permissionService,
            ICustomerTagService customerTagService,
            IExportManager exportManager)
		{
            this._campaignService = campaignService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._localizationService = localizationService;
            this._messageTokenProvider = messageTokenProvider;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._permissionService = permissionService;
            this._customerTagService = customerTagService;
            this._exportManager = exportManager;

        }

        [NonAction]
        protected virtual string FormatTokens(string[] tokens)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                sb.Append(token);
                if (i != tokens.Length - 1)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        [NonAction]
        protected virtual void PrepareStoresModel(CampaignModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Common.All"),
                Value = ""
            });
            var stores = _storeService.GetAllStores();
            foreach (var store in stores)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString()
                });
            }
        }

        [NonAction]
        protected virtual void PrepareCustomerTagsModel(CampaignModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            model.AvailableCustomerTags = _customerTagService.GetAllCustomerTags().Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id, Selected = model.CustomerTags.Contains(ct.Id) }).ToList();

        }

        [NonAction]
        protected virtual void PrepareEmailAccounts(CampaignModel model)
        {
            //available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            return View();
		}

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaigns = _campaignService.GetAllCampaigns();
            var gridModel = new DataSourceResult
            {
                Data = campaigns.Select(x =>
                {
                    var model = x.ToModel();
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return model;
                }),
                Total = campaigns.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult Customers(string campaignId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(campaignId);
            var customers = _campaignService.CustomerSubscriptions(campaign, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers,
                Total = customers.Count
            };
            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult History(string campaignId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(campaignId);
            var history = _campaignService.GetCampaignHistory(campaign, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = history.Select(x => new {
                    Email = x.Email,
                    SentDate = x.CreatedDateUtc,
                }),
                Total = history.Count
            };
            return Json(gridModel);
        }

        public ActionResult ExportCsv(string campaignId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            try
            {
                var campaign = _campaignService.GetCampaignById(campaignId);
                var customers = _campaignService.CustomerSubscriptions(campaign);
                string result = _exportManager.ExportNewsletterSubscribersToTxt(customers.Select(x=>x.Email).ToList());

                string fileName = String.Format("newsletter_emails_campaign_{0}_{1}.txt", campaign.Name, CommonHelper.GenerateRandomDigitCode(4));
                return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }



        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var model = new CampaignModel();
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity();
                campaign.CreatedOnUtc = DateTime.UtcNow;
                _campaignService.InsertCampaign(campaign);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);

            return View(model);
        }

		public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            var model = campaign.ToModel();
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);
            return View(model);
		}

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                _campaignService.UpdateCampaign(campaign);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Updated"));
                //selected tab
                SaveSelectedTabIndex();

                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);
            return View(model);
		}

        [HttpPost,ActionName("Edit")]
        [FormValueRequired("send-test-email")]
        public ActionResult SendTestEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");


            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);

            try
            {
                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                if (emailAccount == null)
                    throw new NopException("Email account could not be loaded");


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
        public ActionResult SendMassEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");


            model.AllowedTokens = FormatTokens(_messageTokenProvider.GetListOfCampaignAllowedTokens());
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //email
            PrepareEmailAccounts(model);

            model.CustomerTags = campaign.CustomerTags.ToList();
            try
            {
                var emailAccount = _emailAccountService.GetEmailAccountById(campaign.EmailAccountId);
                if (emailAccount == null)
                    throw new NopException("Email account could not be loaded");

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
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                //No campaign found with the specified id
                return RedirectToAction("List");

            _campaignService.DeleteCampaign(campaign);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Deleted"));
			return RedirectToAction("List");
		}
	}
}
