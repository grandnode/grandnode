using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Knowledgebase;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using Grand.Services.Localization;
using Grand.Web.Infrastructure.Cache;
using Grand.Core;
using Grand.Services.Customers;
using Grand.Core.Caching;
using Grand.Services.Security;
using Grand.Services.Stores;
using System.Linq;
using Grand.Services.Seo;

namespace Grand.Web.Controllers
{
    public class KnowledgebaseController : BasePublicController
    {
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;

        public KnowledgebaseController(KnowledgebaseSettings knowledgebaseSettings, IKnowledgebaseService knowledgebaseService, IWorkContext workContext,
            IStoreContext storeContext, ICacheManager cacheManager, IAclService aclService, IStoreMappingService storeMappingService)
        {
            this._knowledgebaseSettings = knowledgebaseSettings;
            this._knowledgebaseService = knowledgebaseService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
        }

        public IActionResult List()
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            return View("List", model);
        }

        public IActionResult ArticlesByCategory(string categoryId)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var category = _knowledgebaseService.GetPublicKnowledgebaseCategory(categoryId);
            if (category == null)
                return RedirectToAction("List");

            var model = new KnowledgebaseHomePageModel();
            var articles = _knowledgebaseService.GetPublicKnowledgebaseArticlesByCategory(categoryId);
            var allCategories = _knowledgebaseService.GetPublicKnowledgebaseCategories();
            articles.ForEach(x => model.Items.Add(new KnowledgebaseItemModel
            {
                Name = x.GetLocalized(y => y.Name),
                Id = x.Id,
                SeName = x.GetLocalized(y => y.SeName),
                IsArticle = true
            }));

            model.CurrentCategoryId = categoryId;

            model.CurrentCategoryDescription = category.GetLocalized(y => y.Description);
            model.CurrentCategoryMetaDescription = category.GetLocalized(y => y.MetaDescription);
            model.CurrentCategoryMetaKeywords = category.GetLocalized(y => y.MetaKeywords);
            model.CurrentCategoryMetaTitle = category.GetLocalized(y => y.MetaTitle);
            model.CurrentCategoryName = category.GetLocalized(y => y.Name);
            model.CurrentCategorySeName = category.GetLocalized(y => y.SeName);

            string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.KNOWLEDGEBASE_CATEGORY_BREADCRUMB_KEY,
            category.Id,
            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
            _storeContext.CurrentStore.Id,
            _workContext.WorkingLanguage.Id);
            model.CategoryBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                category
                .GetCategoryBreadCrumb(_knowledgebaseService, _aclService, _storeMappingService)
                .Select(catBr => new KnowledgebaseCategoryModel
                {
                    Id = catBr.Id,
                    Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id)
                })
                .ToList()
            );

            return View("List", model);
        }

        public IActionResult ItemsByKeyword(string keyword)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            if (!string.IsNullOrEmpty(keyword))
            {
                var categories = _knowledgebaseService.GetPublicKnowledgebaseCategoriesByKeyword(keyword);
                var allCategories = _knowledgebaseService.GetPublicKnowledgebaseCategories();
                categories.ForEach(x => model.Items.Add(new KnowledgebaseItemModel
                {
                    Name = x.GetLocalized(y => y.Name),
                    Id = x.Id,
                    SeName = x.GetLocalized(y => y.SeName),
                    IsArticle = false,
                    FormattedBreadcrumbs = x.GetFormattedBreadCrumb(allCategories, ">")
                }));

                var articles = _knowledgebaseService.GetPublicKnowledgebaseArticlesByKeyword(keyword);
                articles.ForEach(x => model.Items.Add(new KnowledgebaseItemModel
                {
                    Name = x.GetLocalized(y => y.Name),
                    Id = x.Id,
                    SeName = x.GetLocalized(y => y.SeName),
                    IsArticle = true,
                    FormattedBreadcrumbs = _knowledgebaseService.GetPublicKnowledgebaseCategory(x.ParentCategoryId)?.GetFormattedBreadCrumb(allCategories, ">") +
                    " > " + x.GetLocalized(y => y.Name)
                }));
            }

            model.CurrentCategoryId = "[NONE]";
            model.SearchKeyword = keyword;

            return View("List", model);
        }

        public IActionResult KnowledgebaseArticle(string articleId)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseArticleModel();
            var article = _knowledgebaseService.GetPublicKnowledgebaseArticle(articleId);
            if (article == null)
                return RedirectToAction("List");

            model.Content = article.GetLocalized(y => y.Content);
            model.Name = article.GetLocalized(y => y.Name);
            model.Id = article.Id;
            model.ParentCategoryId = article.ParentCategoryId;
            model.SeName = article.GetLocalized(y => y.SeName);

            foreach (var id in article.RelatedArticles)
            {
                var a = _knowledgebaseService.GetPublicKnowledgebaseArticle(id);
                if (a != null)
                    model.RelatedArticles.Add(new KnowledgebaseArticleModel
                    {
                        SeName = a.SeName,
                        Id = a.Id,
                        Name = a.Name
                    });
            }

            var category = _knowledgebaseService.GetKnowledgebaseCategory(article.ParentCategoryId);
            if (category != null)
            {
                string breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.KNOWLEDGEBASE_CATEGORY_BREADCRUMB_KEY,
                article.ParentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id);
                model.CategoryBreadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                    category
                    .GetCategoryBreadCrumb(_knowledgebaseService, _aclService, _storeMappingService)
                    .Select(catBr => new KnowledgebaseCategoryModel
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id)
                    })
                    .ToList()
                );
            }

            return View("Article", model);
        }
    }
}
