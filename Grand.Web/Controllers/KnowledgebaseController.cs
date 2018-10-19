using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Infrastructure;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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
        private readonly ILocalizationService _localizationService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IPictureService _pictureService;

        public KnowledgebaseController(KnowledgebaseSettings knowledgebaseSettings, IKnowledgebaseService knowledgebaseService, IWorkContext workContext,
            IStoreContext storeContext, ICacheManager cacheManager, IAclService aclService, IStoreMappingService storeMappingService, ILocalizationService localizationService,
            CaptchaSettings captchaSettings, LocalizationSettings localizationSettings, IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService, IDateTimeHelper dateTimeHelper, CustomerSettings customerSettings,
            MediaSettings mediaSettings, IPictureService pictureService)
        {
            this._knowledgebaseSettings = knowledgebaseSettings;
            this._knowledgebaseService = knowledgebaseService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._cacheManager = cacheManager;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._localizationService = localizationService;
            this._captchaSettings = captchaSettings;
            this._localizationSettings = localizationSettings;
            this._workflowMessageService = workflowMessageService;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerSettings = customerSettings;
            this._mediaSettings = mediaSettings;
            this._pictureService = pictureService;
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

            PrepareKnowledgebaseArticleModel(model, article);
            return View("Article", model);
        }

        private void PrepareKnowledgebaseArticleModel(KnowledgebaseArticleModel model, KnowledgebaseArticle article)
        {
            model.Content = article.GetLocalized(y => y.Content);
            model.Name = article.GetLocalized(y => y.Name);
            model.Id = article.Id;
            model.ParentCategoryId = article.ParentCategoryId;
            model.SeName = article.GetLocalized(y => y.SeName);
            model.AllowComments = article.AllowComments;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnArticleCommentPage;
            var articleComments = _knowledgebaseService.GetArticleCommentsByArticleId(article.Id);
            foreach (var ac in articleComments)
            {
                var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(ac.CustomerId);
                var commentModel = new KnowledgebaseArticleCommentModel
                {
                    Id = ac.Id,
                    CustomerId = ac.CustomerId,
                    CustomerName = customer.FormatUserName(),
                    CommentText = ac.CommentText,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(ac.CreatedOnUtc, DateTimeKind.Utc),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                };
                if (_customerSettings.AllowCustomersToUploadAvatars)
                {
                    commentModel.CustomerAvatarUrl = _pictureService.GetPictureUrl(
                        customer.GetAttribute<string>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        _customerSettings.DefaultAvatarEnabled,
                        defaultPictureType: PictureType.Avatar);
                }

                model.Comments.Add(commentModel);
            }

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
        }

        [HttpPost, ActionName("KnowledgebaseArticle")]
        [PublicAntiForgery]
        [FormValueRequired("add-comment")]
        [ValidateCaptcha]
        public virtual IActionResult ArticleCommentAdd(string articleId, KnowledgebaseArticleModel model, bool captchaValid,
               [FromServices] IWorkContext workContext)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var article = _knowledgebaseService.GetPublicKnowledgebaseArticle(articleId);
            if (article == null || !article.AllowComments)
                return RedirectToRoute("HomePage");

            if (workContext.CurrentCustomer.IsGuest() && !_knowledgebaseSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Knowledgebase.Article.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnArticleCommentPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                var customer = _workContext.CurrentCustomer;
                var comment = new KnowledgebaseArticleComment
                {
                    ArticleId = article.Id,
                    CustomerId = customer.Id,
                    CommentText = model.AddNewComment.CommentText,
                    CreatedOnUtc = DateTime.UtcNow,
                    ArticleTitle = article.Name,
                };
                _knowledgebaseService.InsertArticleComment(comment);

                if (!customer.HasContributions)
                {
                    EngineContext.Current.Resolve<ICustomerService>().UpdateContributions(customer);
                }

                //notify a store owner
                if (_knowledgebaseSettings.NotifyAboutNewArticleComments)
                    _workflowMessageService.SendArticleCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddArticleComment", comment.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddArticleComment"));

                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.knowledgebase.addarticlecomment.result"] = _localizationService.GetResource("Knowledgebase.Article.Comments.SuccessfullyAdded");
                return RedirectToRoute("KnowledgebaseArticle", new { SeName = article.GetSeName() });
            }

            //If we got this far, something failed, redisplay form
            PrepareKnowledgebaseArticleModel(model, article);
            return View("Article", model);
        }
    }
}
