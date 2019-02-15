using Grand.Core.Domain.Messages;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CampaignViewModelService: ICampaignViewModelService
    {

        private readonly ICampaignService _campaignService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly ICustomerTagService _customerTagService;
        private readonly INewsletterCategoryService _newsletterCategoryService;

        public CampaignViewModelService(ICampaignService campaignService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            ICustomerTagService customerTagService,
            INewsletterCategoryService newsletterCategoryService)
        {
            _campaignService = campaignService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _emailAccountService = emailAccountService;
            _localizationService = localizationService;
            _messageTokenProvider = messageTokenProvider;
            _storeService = storeService;
            _customerTagService = customerTagService;
            _newsletterCategoryService = newsletterCategoryService;
        }

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

        protected virtual void PrepareCustomerTagsModel(CampaignModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            model.AvailableCustomerTags = _customerTagService.GetAllCustomerTags().Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id, Selected = model.CustomerTags.Contains(ct.Id) }).ToList();
            model.CustomerTags = model.CustomerTags == null ? new List<string>() : model.CustomerTags;
        }

        protected virtual void PrepareCustomerRolesModel(CampaignModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            model.AvailableCustomerRoles = _customerService.GetAllCustomerRoles().Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id, Selected = model.CustomerRoles.Contains(ct.Id) }).ToList();
            model.CustomerRoles = model.CustomerRoles == null ? new List<string>() : model.CustomerRoles;
        }

        protected virtual void PrepareNewsletterCategoriesModel(CampaignModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            model.AvailableNewsletterCategories = _newsletterCategoryService.GetAllNewsletterCategory().Select(ct => new SelectListItem() { Text = ct.Name, Value = ct.Id, Selected = model.NewsletterCategories.Contains(ct.Id) }).ToList();
            model.NewsletterCategories = model.NewsletterCategories == null ? new List<string>() : model.NewsletterCategories;
        }

        protected virtual void PrepareEmailAccounts(CampaignModel model)
        {
            //available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
        }

        public virtual CampaignModel PrepareCampaignModel()
        {
            var model = new CampaignModel();
            model.AllowedTokens = _messageTokenProvider.GetListOfCampaignAllowedTokens();
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //Roles
            PrepareCustomerRolesModel(model);
            //Newsletter categories
            PrepareNewsletterCategoriesModel(model);
            //email
            PrepareEmailAccounts(model);
            return model;
        }

        public virtual CampaignModel PrepareCampaignModel(CampaignModel model)
        {
            //If we got this far, something failed, redisplay form
            model.AllowedTokens = _messageTokenProvider.GetListOfCampaignAllowedTokens();
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //Newsletter categories
            PrepareNewsletterCategoriesModel(model);
            //Roles
            PrepareCustomerRolesModel(model);
            //email
            PrepareEmailAccounts(model);
            return model;
        }

        public virtual CampaignModel PrepareCampaignModel(Campaign campaign)
        {
            var model = campaign.ToModel();
            model.AllowedTokens = _messageTokenProvider.GetListOfCampaignAllowedTokens();
            //stores
            PrepareStoresModel(model);
            //Tags
            PrepareCustomerTagsModel(model);
            //Newsletter categories
            PrepareNewsletterCategoriesModel(model);
            //Roles
            PrepareCustomerRolesModel(model);
            //email
            PrepareEmailAccounts(model);

            return model;
        }

        public virtual Campaign InsertCampaignModel(CampaignModel model)
        {
            var campaign = model.ToEntity();
            campaign.CreatedOnUtc = DateTime.UtcNow;
            _campaignService.InsertCampaign(campaign);
            return campaign;
        }
        public virtual Campaign UpdateCampaignModel(Campaign campaign, CampaignModel model)
        {
            campaign = model.ToEntity(campaign);
            campaign.CustomerRoles.Clear();
            foreach (var item in model.CustomerRoles)
            {
                campaign.CustomerRoles.Add(item);
            }
            campaign.CustomerTags.Clear();
            foreach (var item in model.CustomerTags)
            {
                campaign.CustomerTags.Add(item);
            }
            campaign.NewsletterCategories.Clear();
            foreach (var item in model.NewsletterCategories)
            {
                campaign.NewsletterCategories.Add(item);
            }
            _campaignService.UpdateCampaign(campaign);

            return campaign;
        }
        public virtual (IEnumerable<CampaignModel> campaignModels, int totalCount) PrepareCampaignModels()
        {
            var campaigns = _campaignService.GetAllCampaigns();
            return (campaigns.Select(x => {
                    var model = x.ToModel();
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return model;
                }), campaigns.Count);
        }
    }
}
