using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Knowledgebase)]
    public class KnowledgebaseController : BaseAdminController
    {
        private readonly IKnowledgebaseViewModelService _knowledgebaseViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        public KnowledgebaseController(IKnowledgebaseViewModelService knowledgebaseViewModelService, ILocalizationService localizationService,
            IKnowledgebaseService knowledgebaseService, ILanguageService languageService, 
            ICustomerService customerService, IStoreService storeService)
        {
            this._knowledgebaseViewModelService = knowledgebaseViewModelService;
            this._localizationService = localizationService;
            this._knowledgebaseService = knowledgebaseService;
            this._languageService = languageService;
            this._customerService = customerService;
            this._storeService = storeService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        public IActionResult NodeList()
        {
            var model = _knowledgebaseViewModelService.PrepareTreeNode();
            return Json(model);
        }

        public IActionResult ArticleList(DataSourceRequest command, string parentCategoryId)
        {
            var articles = _knowledgebaseViewModelService.PrepareKnowledgebaseArticleGridModel(parentCategoryId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = articles.knowledgebaseArticleGridModels.ToList(),
                Total = articles.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListCategoryActivityLog(DataSourceRequest command, string categoryId)
        {
            var activityLog = _knowledgebaseViewModelService.PrepareCategoryActivityLogModels(categoryId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModels.ToList(),
                Total = activityLog.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListArticleActivityLog(DataSourceRequest command, string articleId)
        {
            var activityLog = _knowledgebaseViewModelService.PrepareArticleActivityLogModels(articleId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLog.activityLogModels.ToList(),
                Total = activityLog.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult CreateCategory()
        {
            var model = _knowledgebaseViewModelService.PrepareKnowledgebaseCategoryModel();
            model.PrepareACLModel(null, false, _customerService);
            model.PrepareStoresMappingModel(null, false, _storeService);
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var knowledgebaseCategory = _knowledgebaseViewModelService.InsertKnowledgebaseCategoryModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Added"));
                return continueEditing ? RedirectToAction("EditCategory", new { knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //ACL
            model.PrepareACLModel(null, true, _customerService);
            return View(model);
        }

        public IActionResult EditCategory(string id)
        {
            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            var model = knowledgebaseCategory.ToModel();
            _knowledgebaseViewModelService.PrepareCategory(model);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseCategory.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = knowledgebaseCategory.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaDescription = knowledgebaseCategory.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseCategory.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseCategory.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseCategory.GetSeName(languageId, false, false);
            });

            model.PrepareACLModel(knowledgebaseCategory, false, _customerService);
            model.SelectedCustomerRoleIds = knowledgebaseCategory.CustomerRoles.ToArray();

            //Stores
            model.PrepareStoresMappingModel(knowledgebaseCategory, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(model.Id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseCategory = _knowledgebaseViewModelService.UpdateKnowledgebaseCategoryModel(knowledgebaseCategory, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Updated"));
                return continueEditing ? RedirectToAction("EditCategory", new { id = knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _knowledgebaseViewModelService.PrepareCategory(model);
            //Stores
            model.PrepareStoresMappingModel(knowledgebaseCategory, true, _storeService);
            //ACL
            model.PrepareACLModel(knowledgebaseCategory, true, _customerService);

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteCategory(string id)
        {
            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (_knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(id).Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Cannotdeletewitharticles"));
                return RedirectToAction("EditCategory", new { id });
            }

            if (ModelState.IsValid)
            {
                _knowledgebaseViewModelService.DeleteKnowledgebaseCategoryModel(knowledgebaseCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("EditCategory", new { id });
        }

        public IActionResult CreateArticle(string parentCategoryId)
        {
            var model = _knowledgebaseViewModelService.PrepareKnowledgebaseArticleModel();
            //ACL
            model.PrepareACLModel(null, false, _customerService);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);

            if (!string.IsNullOrEmpty(parentCategoryId))
                model.ParentCategoryId = parentCategoryId;

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var knowledgebaseArticle = _knowledgebaseViewModelService.InsertKnowledgebaseArticleModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Added"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            _knowledgebaseViewModelService.PrepareCategory(model);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //ACL
            model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        public IActionResult EditArticle(string id)
        {
            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            var model = knowledgebaseArticle.ToModel();
            _knowledgebaseViewModelService.PrepareCategory(model);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseArticle.GetLocalized(x => x.Name, languageId, false, false);
                locale.Content = knowledgebaseArticle.GetLocalized(x => x.Content, languageId, false, false);
                locale.MetaDescription = knowledgebaseArticle.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseArticle.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseArticle.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseArticle.GetSeName(languageId, false, false);
            });

            model.PrepareACLModel(knowledgebaseArticle, false, _customerService);
            model.SelectedCustomerRoleIds = knowledgebaseArticle.CustomerRoles.ToArray();

            //Stores
            model.PrepareStoresMappingModel(knowledgebaseArticle, false, _storeService);

            model.AllowComments = knowledgebaseArticle.AllowComments;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(model.Id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseArticle = _knowledgebaseViewModelService.UpdateKnowledgebaseArticleModel(knowledgebaseArticle, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Updated"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            _knowledgebaseViewModelService.PrepareCategory(model);
            //Store
            model.PrepareStoresMappingModel(knowledgebaseArticle, true, _storeService);
            //ACL
            model.PrepareACLModel(knowledgebaseArticle, true, _customerService);

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteArticle(string id)
        {
            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _knowledgebaseViewModelService.DeleteKnowledgebaseArticle(knowledgebaseArticle);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("EditArticle", new { knowledgebaseArticle.Id });
        }

        public IActionResult ArticlesPopup(string articleId)
        {
            var model = new KnowledgebaseArticleModel.AddRelatedArticleModel();
            model.ArticleId = articleId;
            model.AvailableArticles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var articles = _knowledgebaseService.GetKnowledgebaseArticles();
            foreach (var a in articles)
                model.AvailableArticles.Add(new SelectListItem { Text = a.Name, Value = a.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult RelatedArticlesAddPopupList(DataSourceRequest command, KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var articles = _knowledgebaseService.GetKnowledgebaseArticlesByName(model.SearchArticleName, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = articles.Select(x => x.ToModel());
            gridModel.Total = articles.TotalCount;

            return Json(gridModel);
        }

        public IActionResult RelatedArticlesList(DataSourceRequest command, string articleId)
        {
            var articles = _knowledgebaseService.GetRelatedKnowledgebaseArticles(articleId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = articles.Select(x => new KnowledgebaseRelatedArticleGridModel
                {
                    Article2Id = x.Id,
                    DisplayOrder = x.DisplayOrder,
                    Published = x.Published,
                    Article2Name = x.Name,
                    Id = x.Id
                }),
                Total = articles.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ArticlesPopup(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            if (model.SelectedArticlesIds != null)
            {
                _knowledgebaseViewModelService.InsertKnowledgebaseRelatedArticle(model);
            }

            ViewBag.RefreshPage = true;

            return View(model);
        }

        [HttpPost]
        public IActionResult RelatedArticleDelete(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            if (model.ArticleId == null || model.Id == null)
                throw new ArgumentNullException("Article id expected ");

            _knowledgebaseViewModelService.DeleteKnowledgebaseRelatedArticle(model);

            return new NullJsonResult();
        }
    }
}
