using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Customers;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Media;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
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
using System.Threading.Tasks;

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
            _knowledgebaseSettings = knowledgebaseSettings;
            _knowledgebaseService = knowledgebaseService;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _localizationService = localizationService;
            _captchaSettings = captchaSettings;
            _localizationSettings = localizationSettings;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
            _pictureService = pictureService;
        }

        public virtual IActionResult List()
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            return View("List", model);
        }

        public virtual async Task<IActionResult> ArticlesByCategory(string categoryId)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var category = await _knowledgebaseService.GetPublicKnowledgebaseCategory(categoryId);
            if (category == null)
                return RedirectToAction("List");

            var model = new KnowledgebaseHomePageModel();
            var articles = await _knowledgebaseService.GetPublicKnowledgebaseArticlesByCategory(categoryId);
            var allCategories = _knowledgebaseService.GetPublicKnowledgebaseCategories();
            articles.ForEach(x => model.Items.Add(new KnowledgebaseItemModel {
                Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                Id = x.Id,
                SeName = x.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id),
                IsArticle = true
            }));

            model.CurrentCategoryId = categoryId;

            model.CurrentCategoryDescription = category.GetLocalized(y => y.Description, _workContext.WorkingLanguage.Id);
            model.CurrentCategoryMetaDescription = category.GetLocalized(y => y.MetaDescription, _workContext.WorkingLanguage.Id);
            model.CurrentCategoryMetaKeywords = category.GetLocalized(y => y.MetaKeywords, _workContext.WorkingLanguage.Id);
            model.CurrentCategoryMetaTitle = category.GetLocalized(y => y.MetaTitle, _workContext.WorkingLanguage.Id);
            model.CurrentCategoryName = category.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id);
            model.CurrentCategorySeName = category.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id);

            string breadcrumbCacheKey = string.Format(ModelCacheEventConst.KNOWLEDGEBASE_CATEGORY_BREADCRUMB_KEY, category.Id,
            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()), _storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
            model.CategoryBreadcrumb = await _cacheManager.GetAsync(breadcrumbCacheKey, async () =>
                (await category.GetCategoryBreadCrumb(_knowledgebaseService, _aclService, _storeMappingService))
                .Select(catBr => new KnowledgebaseCategoryModel {
                    Id = catBr.Id,
                    Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id)
                })
                .ToList()
            );

            return View("List", model);
        }

        public virtual async Task<IActionResult> ItemsByKeyword(string keyword)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            if (!string.IsNullOrEmpty(keyword))
            {
                var categories = await _knowledgebaseService.GetPublicKnowledgebaseCategoriesByKeyword(keyword);
                var allCategories = await _knowledgebaseService.GetPublicKnowledgebaseCategories();
                categories.ForEach(x => model.Items.Add(new KnowledgebaseItemModel {
                    Name = x.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                    Id = x.Id,
                    SeName = x.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id),
                    IsArticle = false,
                    FormattedBreadcrumbs = x.GetFormattedBreadCrumb(allCategories, ">")
                }));

                var articles = await _knowledgebaseService.GetPublicKnowledgebaseArticlesByKeyword(keyword);
                foreach (var item in articles)
                {
                    var kbm = new KnowledgebaseItemModel {
                        Name = item.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                        Id = item.Id,
                        SeName = item.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id),
                        IsArticle = true,
                        FormattedBreadcrumbs = (await _knowledgebaseService.GetPublicKnowledgebaseCategory(item.ParentCategoryId))?.GetFormattedBreadCrumb(allCategories, ">") +
                                        " > " + item.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id)
                    };
                    model.Items.Add(kbm);
                }
            }
            model.CurrentCategoryId = "[NONE]";
            model.SearchKeyword = keyword;

            return View("List", model);
        }

        public virtual async Task<IActionResult> KnowledgebaseArticle(string articleId, [FromServices] ICustomerService customerService)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var customer = _workContext.CurrentCustomer;
            var article = await _knowledgebaseService.GetKnowledgebaseArticle(articleId);
            if (article == null)
                return RedirectToAction("List");

            //ACL (access control list)
            if (!_aclService.Authorize(article, customer))
                return InvokeHttp404();

            //Store mapping
            if (!_storeMappingService.Authorize(article))
                return InvokeHttp404();

            var model = new KnowledgebaseArticleModel();

            await PrepareKnowledgebaseArticleModel(model, article, customerService);
            return View("Article", model);
        }

        private async Task PrepareKnowledgebaseArticleModel(KnowledgebaseArticleModel model, KnowledgebaseArticle article, ICustomerService customerService)
        {
            model.Content = article.GetLocalized(y => y.Content, _workContext.WorkingLanguage.Id);
            model.Name = article.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id);
            model.Id = article.Id;
            model.ParentCategoryId = article.ParentCategoryId;
            model.SeName = article.GetLocalized(y => y.SeName, _workContext.WorkingLanguage.Id);
            model.AllowComments = article.AllowComments;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnArticleCommentPage;
            var articleComments = await _knowledgebaseService.GetArticleCommentsByArticleId(article.Id);
            foreach (var ac in articleComments)
            {
                var customer = await customerService.GetCustomerById(ac.CustomerId);
                var commentModel = new KnowledgebaseArticleCommentModel {
                    Id = ac.Id,
                    CustomerId = ac.CustomerId,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    CommentText = ac.CommentText,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(ac.CreatedOnUtc, DateTimeKind.Utc),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                };
                if (_customerSettings.AllowCustomersToUploadAvatars)
                {
                    commentModel.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                        customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        _customerSettings.DefaultAvatarEnabled,
                        defaultPictureType: PictureType.Avatar);
                }

                model.Comments.Add(commentModel);
            }

            foreach (var id in article.RelatedArticles)
            {
                var a = await _knowledgebaseService.GetPublicKnowledgebaseArticle(id);
                if (a != null)
                    model.RelatedArticles.Add(new KnowledgebaseArticleModel {
                        SeName = a.SeName,
                        Id = a.Id,
                        Name = a.Name
                    });
            }

            var category = await _knowledgebaseService.GetKnowledgebaseCategory(article.ParentCategoryId);
            if (category != null)
            {
                string breadcrumbCacheKey = string.Format(ModelCacheEventConst.KNOWLEDGEBASE_CATEGORY_BREADCRUMB_KEY,
                article.ParentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id);
                model.CategoryBreadcrumb = await _cacheManager.GetAsync(breadcrumbCacheKey, async () =>
                    (await category.GetCategoryBreadCrumb(_knowledgebaseService, _aclService, _storeMappingService))
                    .Select(catBr => new KnowledgebaseCategoryModel {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id)
                    })
                    .ToList()
                );
            }
        }

        [HttpPost, ActionName("KnowledgebaseArticle")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("add-comment")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ArticleCommentAdd(string articleId, KnowledgebaseArticleModel model, bool captchaValid,
               [FromServices] IWorkContext workContext, [FromServices] ICustomerService customerService)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var article = await _knowledgebaseService.GetPublicKnowledgebaseArticle(articleId);
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
                var comment = new KnowledgebaseArticleComment {
                    ArticleId = article.Id,
                    CustomerId = customer.Id,
                    CommentText = model.AddNewComment.CommentText,
                    CreatedOnUtc = DateTime.UtcNow,
                    ArticleTitle = article.Name,
                };
                await _knowledgebaseService.InsertArticleComment(comment);

                if (!customer.HasContributions)
                {
                    await customerService.UpdateContributions(customer);
                }

                //notify a store owner
                if (_knowledgebaseSettings.NotifyAboutNewArticleComments)
                    await _workflowMessageService.SendArticleCommentNotificationMessage(article, comment, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AddArticleComment", comment.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddArticleComment"));

                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.knowledgebase.addarticlecomment.result"] = _localizationService.GetResource("Knowledgebase.Article.Comments.SuccessfullyAdded");
                return RedirectToRoute("KnowledgebaseArticle", new { SeName = article.GetSeName(_workContext.WorkingLanguage.Id) });
            }

            //If we got this far, something failed, redisplay form
            await PrepareKnowledgebaseArticleModel(model, article, customerService);
            return View("Article", model);
        }
    }
}
