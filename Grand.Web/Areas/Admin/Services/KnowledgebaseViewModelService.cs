using Grand.Core.Domain.Knowledgebase;
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

        public KnowledgebaseViewModelService(ILocalizationService localizationService,
            IKnowledgebaseService knowledgebaseService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper, IUrlRecordService urlRecordService)
        {
            _localizationService = localizationService;
            _knowledgebaseService = knowledgebaseService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _urlRecordService = urlRecordService;
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
        public virtual void PrepareCategory(KnowledgebaseCategoryModel model)
        {
            model.Categories.Add(new SelectListItem { Text = "[None]", Value = "" });
            var categories = _knowledgebaseService.GetKnowledgebaseCategories();
            foreach (var category in categories)
            {
                model.Categories.Add(new SelectListItem
                {
                    Value = category.Id,
                    Text = category.GetFormattedBreadCrumb(categories)
                });
            }
        }
        public virtual void PrepareCategory(KnowledgebaseArticleModel model)
        {
            model.Categories.Add(new SelectListItem { Text = "[None]", Value = "" });
            var categories = _knowledgebaseService.GetKnowledgebaseCategories();
            foreach (var category in categories)
            {
                model.Categories.Add(new SelectListItem
                {
                    Value = category.Id,
                    Text = category.GetFormattedBreadCrumb(categories)
                });
            }
        }
        public virtual List<TreeNode> PrepareTreeNode()
        {
            var categories = _knowledgebaseService.GetKnowledgebaseCategories();
            var articles = _knowledgebaseService.GetKnowledgebaseArticles();
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
        public virtual (IEnumerable<KnowledgebaseArticleGridModel> knowledgebaseArticleGridModels, int totalCount) PrepareKnowledgebaseArticleGridModel(string parentCategoryId, int pageIndex, int pageSize)
        {
            var articles = _knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(parentCategoryId, pageIndex - 1, pageSize);
            return (articles.Select(x => new KnowledgebaseArticleGridModel
            {
                Name = x.Name,
                DisplayOrder = x.DisplayOrder,
                Published = x.Published,
                ArticleId = x.Id,
                Id = x.Id
            }), articles.TotalCount);
        }
        public virtual (IEnumerable<KnowledgebaseCategoryModel.ActivityLogModel> activityLogModels, int totalCount) PrepareCategoryActivityLogModels(string categoryId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetKnowledgebaseCategoryActivities(null, null, categoryId, pageIndex - 1, pageSize);

            return (activityLog.Select(x =>
            {
                var customer = _customerService.GetCustomerById(x.CustomerId);
                var m = new KnowledgebaseCategoryModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };

                return m;
            }), activityLog.TotalCount);
        }
        public virtual (IEnumerable<KnowledgebaseArticleModel.ActivityLogModel> activityLogModels, int totalCount) PrepareArticleActivityLogModels(string articleId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetKnowledgebaseArticleActivities(null, null, articleId, pageIndex - 1, pageSize);

            return (activityLog.Select(x =>
            {
                var customer = _customerService.GetCustomerById(x.CustomerId);
                var m = new KnowledgebaseArticleModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                return m;
            }), activityLog.TotalCount);
        }
        public virtual KnowledgebaseCategoryModel PrepareKnowledgebaseCategoryModel()
        {
            var model = new KnowledgebaseCategoryModel();
            PrepareCategory(model);
            return model;
        }
        
        public virtual KnowledgebaseCategory InsertKnowledgebaseCategoryModel(KnowledgebaseCategoryModel model)
        {
            var knowledgebaseCategory = model.ToEntity();
            knowledgebaseCategory.CreatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.Locales = model.Locales.ToLocalizedProperty(knowledgebaseCategory, x => x.Name, _urlRecordService);
            model.SeName = knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true);
            knowledgebaseCategory.SeName = model.SeName;
            _knowledgebaseService.InsertKnowledgebaseCategory(knowledgebaseCategory);

            _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");

            _customerActivityService.InsertActivity("CreateKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.CreateKnowledgebaseCategory"), knowledgebaseCategory.Name);
            return knowledgebaseCategory;
        }
        public virtual KnowledgebaseCategory UpdateKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory, KnowledgebaseCategoryModel model)
        {
            knowledgebaseCategory = model.ToEntity(knowledgebaseCategory);
            knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseCategory.Locales = model.Locales.ToLocalizedProperty(knowledgebaseCategory, x => x.Name, _urlRecordService);
            model.SeName = knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true);
            knowledgebaseCategory.SeName = model.SeName;
            _knowledgebaseService.UpdateKnowledgebaseCategory(knowledgebaseCategory);

            _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");

            _customerActivityService.InsertActivity("UpdateKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseCategory"), knowledgebaseCategory.Name);
            return knowledgebaseCategory;
        }
        public virtual void DeleteKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory)
        {
            _knowledgebaseService.DeleteKnowledgebaseCategory(knowledgebaseCategory);

            _customerActivityService.InsertActivity("DeleteKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseCategory"), knowledgebaseCategory.Name);
        }
        public virtual KnowledgebaseArticleModel PrepareKnowledgebaseArticleModel()
        {
            var model = new KnowledgebaseArticleModel();
            model.Published = true;
            model.AllowComments = true;
            PrepareCategory(model);
            return model;
        }
        public virtual KnowledgebaseArticle InsertKnowledgebaseArticleModel(KnowledgebaseArticleModel model)
        {
            var knowledgebaseArticle = model.ToEntity();
            knowledgebaseArticle.CreatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.Locales = model.Locales.ToLocalizedProperty(knowledgebaseArticle, x => x.Name, _urlRecordService);
            model.SeName = knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true);
            knowledgebaseArticle.SeName = model.SeName;
            knowledgebaseArticle.AllowComments = model.AllowComments;

            _knowledgebaseService.InsertKnowledgebaseArticle(knowledgebaseArticle);

            _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");

            _customerActivityService.InsertActivity("CreateKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.CreateKnowledgebaseArticle"), knowledgebaseArticle.Name);

            return knowledgebaseArticle;
        }
        public virtual KnowledgebaseArticle UpdateKnowledgebaseArticleModel(KnowledgebaseArticle knowledgebaseArticle, KnowledgebaseArticleModel model)
        {
            knowledgebaseArticle = model.ToEntity(knowledgebaseArticle);
            knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
            knowledgebaseArticle.Locales = model.Locales.ToLocalizedProperty(knowledgebaseArticle, x => x.Name, _urlRecordService);
            model.SeName = knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true);
            knowledgebaseArticle.SeName = model.SeName;
            knowledgebaseArticle.AllowComments = model.AllowComments;
            _knowledgebaseService.UpdateKnowledgebaseArticle(knowledgebaseArticle);

            _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");

            _customerActivityService.InsertActivity("UpdateKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseArticle"), knowledgebaseArticle.Name);
            return knowledgebaseArticle;
        }
        public virtual void DeleteKnowledgebaseArticle(KnowledgebaseArticle knowledgebaseArticle)
        {
            _knowledgebaseService.DeleteKnowledgebaseArticle(knowledgebaseArticle);

            _customerActivityService.InsertActivity("DeleteKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseArticle"), knowledgebaseArticle.Name);
        }
        public virtual void InsertKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var article = _knowledgebaseService.GetKnowledgebaseArticle(model.ArticleId);

            foreach (var id in model.SelectedArticlesIds)
            {
                if (id != article.Id)
                    if (!article.RelatedArticles.Contains(id))
                        article.RelatedArticles.Add(id);
            }

            _knowledgebaseService.UpdateKnowledgebaseArticle(article);
        }
        public virtual void DeleteKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var article = _knowledgebaseService.GetKnowledgebaseArticle(model.ArticleId);
            var related = _knowledgebaseService.GetKnowledgebaseArticle(model.Id);

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

            _knowledgebaseService.UpdateKnowledgebaseArticle(article);
        }
    }
}
