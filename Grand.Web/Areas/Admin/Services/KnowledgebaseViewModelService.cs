using Grand.Domain.Knowledgebase;
using Grand.Domain.Seo;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Seo;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class KnowledgebaseViewModelService : IKnowledgebaseViewModelService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSettings;

        public KnowledgebaseViewModelService(ILocalizationService localizationService,
            IKnowledgebaseService knowledgebaseService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper, IUrlRecordService urlRecordService,
            ILanguageService languageService, SeoSettings seoSettings)
        {
            _localizationService = localizationService;
            _knowledgebaseService = knowledgebaseService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _seoSettings = seoSettings;
        }

        protected virtual void FillChildNodes(TreeNode parentNode, List<ITreeNode> nodes)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.id);
            foreach (var child in children)
            {
                var newNode = new TreeNode
                {
                    id = child.Id,
                    text = child.Name,
                    isCategory = child.GetType() == typeof(KnowledgebaseCategory),
                    nodes = new List<TreeNode>()
                };

                FillChildNodes(newNode, nodes);

                parentNode.nodes.Add(newNode);
            }
        }
        public virtual async Task PrepareCategory(KnowledgebaseCategoryModel model)
        {
            model.Categories.Add(new SelectListItem { Text = "[None]", Value = "" });
            var categories = await _knowledgebaseService.GetKnowledgebaseCategories();
            foreach (var category in categories)
            {
                model.Categories.Add(new SelectListItem
                {
                    Value = category.Id,
                    Text = category.GetFormattedBreadCrumb(categories)
                });
            }
        }
        public virtual async Task PrepareCategory(KnowledgebaseArticleModel model)
        {
            model.Categories.Add(new SelectListItem { Text = "[None]", Value = "" });
            var categories = await _knowledgebaseService.GetKnowledgebaseCategories();
            foreach (var category in categories)
            {
                model.Categories.Add(new SelectListItem
                {
                    Value = category.Id,
                    Text = category.GetFormattedBreadCrumb(categories)
                });
            }
        }
        public virtual async Task<List<TreeNode>> PrepareTreeNode()
        {
            var categories = await _knowledgebaseService.GetKnowledgebaseCategories();
            var articles = await _knowledgebaseService.GetKnowledgebaseArticles();
            List<TreeNode> nodeList = new List<TreeNode>();

            List<ITreeNode> list = new List<ITreeNode>();
            list.AddRange(categories);
            list.AddRange(articles);

            foreach (var node in list)
            {
                if (string.IsNullOrEmpty(node.ParentCategoryId))
                {
                    var newNode = new TreeNode
                    {
                        id = node.Id,
                        text = node.Name,
                        isCategory = node.GetType() == typeof(KnowledgebaseCategory),
                        nodes = new List<TreeNode>()
                    };

                    FillChildNodes(newNode, list);

                    nodeList.Add(newNode);
                }
            }
            return nodeList;
        }
        public virtual async Task<(IEnumerable<KnowledgebaseArticleGridModel> knowledgebaseArticleGridModels, int totalCount)> PrepareKnowledgebaseArticleGridModel(string parentCategoryId, int pageIndex, int pageSize)
        {
            var articles = await _knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(parentCategoryId, pageIndex - 1, pageSize);
            return (articles.Select(x => new KnowledgebaseArticleGridModel
            {
                Name = x.Name,
                DisplayOrder = x.DisplayOrder,
                Published = x.Published,
                ArticleId = x.Id,
                Id = x.Id
            }), articles.TotalCount);
        }
        public virtual async Task<(IEnumerable<KnowledgebaseCategoryModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareCategoryActivityLogModels(string categoryId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetKnowledgebaseCategoryActivities(null, null, categoryId, pageIndex - 1, pageSize);
            var items = new List<KnowledgebaseCategoryModel.ActivityLogModel>();
            foreach (var x in activityLog)
            {
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                var m = new KnowledgebaseCategoryModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId))?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                items.Add(m);
            }
            return (items, activityLog.TotalCount);
        }
        public virtual async Task<(IEnumerable<KnowledgebaseArticleModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareArticleActivityLogModels(string articleId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetKnowledgebaseArticleActivities(null, null, articleId, pageIndex - 1, pageSize);
            var items = new List<KnowledgebaseArticleModel.ActivityLogModel>();
            foreach (var x in activityLog)
            {
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                var m = new KnowledgebaseArticleModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId))?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                items.Add(m);
            }
            return (items, activityLog.TotalCount);
        }
        public virtual async Task<KnowledgebaseCategoryModel> PrepareKnowledgebaseCategoryModel()
        {
            var model = new KnowledgebaseCategoryModel();
            model.Published = true;
            await PrepareCategory(model);
            return model;
        }
        
        public virtual async Task<KnowledgebaseCategory> InsertKnowledgebaseCategoryModel(KnowledgebaseCategoryModel model)
        {
            var knowledgebaseCategory = model.ToEntity();
            knowledgebaseCategory.CreatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.Locales = await model.Locales.ToLocalizedProperty(knowledgebaseCategory, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true, _seoSettings, _urlRecordService, _languageService);
            knowledgebaseCategory.SeName = model.SeName;
            await _knowledgebaseService.InsertKnowledgebaseCategory(knowledgebaseCategory);
            await _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");
            await _customerActivityService.InsertActivity("CreateKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.CreateKnowledgebaseCategory"), knowledgebaseCategory.Name);

            return knowledgebaseCategory;
        }
        public virtual async Task<KnowledgebaseCategory> UpdateKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory, KnowledgebaseCategoryModel model)
        {
            knowledgebaseCategory = model.ToEntity(knowledgebaseCategory);
            knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.Locales = await model.Locales.ToLocalizedProperty(knowledgebaseCategory, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true, _seoSettings, _urlRecordService, _languageService);
            knowledgebaseCategory.SeName = model.SeName;
            await _knowledgebaseService.UpdateKnowledgebaseCategory(knowledgebaseCategory);
            await _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");
            await _customerActivityService.InsertActivity("UpdateKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseCategory"), knowledgebaseCategory.Name);

            return knowledgebaseCategory;
        }
        public virtual async Task DeleteKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory)
        {
            await _knowledgebaseService.DeleteKnowledgebaseCategory(knowledgebaseCategory);
            await _customerActivityService.InsertActivity("DeleteKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseCategory"), knowledgebaseCategory.Name);
        }
        public virtual async Task<KnowledgebaseArticleModel> PrepareKnowledgebaseArticleModel()
        {
            var model = new KnowledgebaseArticleModel
            {
                Published = true,
                AllowComments = true
            };
            await PrepareCategory(model);
            return model;
        }
        public virtual async Task<KnowledgebaseArticle> InsertKnowledgebaseArticleModel(KnowledgebaseArticleModel model)
        {
            var knowledgebaseArticle = model.ToEntity();
            knowledgebaseArticle.CreatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.Locales = await model.Locales.ToLocalizedProperty(knowledgebaseArticle, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true, _seoSettings, _urlRecordService, _languageService);
            knowledgebaseArticle.SeName = model.SeName;
            knowledgebaseArticle.AllowComments = model.AllowComments;
            await _knowledgebaseService.InsertKnowledgebaseArticle(knowledgebaseArticle);
            await _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");
            await _customerActivityService.InsertActivity("CreateKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.CreateKnowledgebaseArticle"), knowledgebaseArticle.Name);

            return knowledgebaseArticle;
        }
        public virtual async Task<KnowledgebaseArticle> UpdateKnowledgebaseArticleModel(KnowledgebaseArticle knowledgebaseArticle, KnowledgebaseArticleModel model)
        {
            knowledgebaseArticle = model.ToEntity(knowledgebaseArticle);
            knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.Locales = await model.Locales.ToLocalizedProperty(knowledgebaseArticle, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true, _seoSettings, _urlRecordService, _languageService);
            knowledgebaseArticle.SeName = model.SeName;
            knowledgebaseArticle.AllowComments = model.AllowComments;
            await _knowledgebaseService.UpdateKnowledgebaseArticle(knowledgebaseArticle);
            await _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");
            await _customerActivityService.InsertActivity("UpdateKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseArticle"), knowledgebaseArticle.Name);

            return knowledgebaseArticle;
        }
        public virtual async Task DeleteKnowledgebaseArticle(KnowledgebaseArticle knowledgebaseArticle)
        {
            await _knowledgebaseService.DeleteKnowledgebaseArticle(knowledgebaseArticle);
            await _customerActivityService.InsertActivity("DeleteKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseArticle"), knowledgebaseArticle.Name);
        }
        public virtual async Task InsertKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var article = await _knowledgebaseService.GetKnowledgebaseArticle(model.ArticleId);

            foreach (var id in model.SelectedArticlesIds)
            {
                if (id != article.Id)
                    if (!article.RelatedArticles.Contains(id))
                        article.RelatedArticles.Add(id);
            }
            await _knowledgebaseService.UpdateKnowledgebaseArticle(article);
        }
        public virtual async Task DeleteKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var article = await _knowledgebaseService.GetKnowledgebaseArticle(model.ArticleId);
            var related = await _knowledgebaseService.GetKnowledgebaseArticle(model.Id);

            if (article == null || related == null)
                throw new ArgumentNullException("No article found with specified id");

            string toDelete = "";
            foreach (var item in article.RelatedArticles)
            {
                if (item == related.Id)
                    toDelete = item;
            }

            if (!string.IsNullOrEmpty(toDelete))
                article.RelatedArticles.Remove(toDelete);

            await _knowledgebaseService.UpdateKnowledgebaseArticle(article);
        }
    }
}
