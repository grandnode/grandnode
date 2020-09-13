using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.News;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using Grand.Framework.Components;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Topics;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class FooterViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly IPermissionService _permissionService;

        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ForumSettings _forumSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly BlogSettings _blogSettings;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly NewsSettings _newsSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CommonSettings _commonSettings;

        public FooterViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            ITopicService topicService,
            IPermissionService permissionService,
            StoreInformationSettings storeInformationSettings,
            ForumSettings forumSettings,
            CatalogSettings catalogSettings,
            BlogSettings blogSettings,
            KnowledgebaseSettings knowledgebaseSettings,
            NewsSettings newsSettings,
            VendorSettings vendorSettings,
            CommonSettings commonSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _topicService = topicService;
            _permissionService = permissionService;

            _storeInformationSettings = storeInformationSettings;
            _forumSettings = forumSettings;
            _catalogSettings = catalogSettings;
            _blogSettings = blogSettings;
            _knowledgebaseSettings = knowledgebaseSettings;
            _newsSettings = newsSettings;
            _vendorSettings = vendorSettings;
            _commonSettings = commonSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareFooter();
            return View(model);
        }
        private async Task<FooterModel> PrepareFooter()
        {
            var topicModel = (await _topicService.GetAllTopics(_storeContext.CurrentStore.Id))
                .Where(t => (t.IncludeInFooterRow1 || t.IncludeInFooterRow2 || t.IncludeInFooterRow3) && t.Published)
                .Select(t => new FooterModel.FooterTopicModel {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id),
                    SeName = t.GetSeName(_workContext.WorkingLanguage.Id),
                    IncludeInFooterRow1 = t.IncludeInFooterRow1,
                    IncludeInFooterRow2 = t.IncludeInFooterRow2,
                    IncludeInFooterRow3 = t.IncludeInFooterRow3
                }).ToList();

            //model
            var currentstore = _storeContext.CurrentStore;
            var model = new FooterModel {
                StoreName = currentstore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                CompanyName = currentstore.CompanyName,
                CompanyEmail = currentstore.CompanyEmail,
                CompanyAddress = currentstore.CompanyAddress,
                CompanyPhone = currentstore.CompanyPhoneNumber,
                CompanyHours = currentstore.CompanyHours,
                PrivacyPreference = _storeInformationSettings.DisplayPrivacyPreference,
                WishlistEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                ShoppingCartEnabled = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                SitemapEnabled = _commonSettings.SitemapEnabled,
                WorkingLanguageId = _workContext.WorkingLanguage.Id,
                FacebookLink = _storeInformationSettings.FacebookLink,
                TwitterLink = _storeInformationSettings.TwitterLink,
                YoutubeLink = _storeInformationSettings.YoutubeLink,
                InstagramLink = _storeInformationSettings.InstagramLink,
                LinkedInLink = _storeInformationSettings.LinkedInLink,
                PinterestLink = _storeInformationSettings.PinterestLink,
                BlogEnabled = _blogSettings.Enabled,
                KnowledgebaseEnabled = _knowledgebaseSettings.Enabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                NewsEnabled = _newsSettings.Enabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                RecommendedProductsEnabled = _catalogSettings.RecommendedProductsEnabled,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                DisplayTaxShippingInfoFooter = _catalogSettings.DisplayTaxShippingInfoFooter,
                InclTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax,
                HidePoweredByGrandNode = _storeInformationSettings.HidePoweredByGrandNode,
                AllowCustomersToApplyForVendorAccount = _vendorSettings.AllowCustomersToApplyForVendorAccount,
                Topics = topicModel
            };

            return model;
        }


    }
}