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
using System.Threading.Tasks;

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
            _knowledgebaseViewModelService = knowledgebaseViewModelService;
            _localizationService = localizationService;
            _knowledgebaseService = knowledgebaseService;
            _languageService = languageService;
            _customerService = customerService;
            _storeService = storeService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        public async Task<IActionResult> NodeList()
        {
            var model = await _knowledgebaseViewModelService.PrepareTreeNode();
            return Json(model);
        }

        public async Task<IActionResult> ArticleList(DataSourceRequest command, string parentCategoryId)
        {
            var (knowledgebaseArticleGridModels, totalCount) = await _knowledgebaseViewModelService.PrepareKnowledgebaseArticleGridModel(parentCategoryId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = knowledgebaseArticleGridModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> ListCategoryActivityLog(DataSourceRequest command, string categoryId)
        {
            var (activityLogModels, totalCount) = await _knowledgebaseViewModelService.PrepareCategoryActivityLogModels(categoryId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLogModels.ToList(),
                Total = totalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> ListArticleActivityLog(DataSourceRequest command, string articleId)
        {
            var (activityLogModels, totalCount) = await _knowledgebaseViewModelService.PrepareArticleActivityLogModels(articleId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLogModels.ToList(),
                Total = totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> CreateCategory()
        {
            var model = await _knowledgebaseViewModelService.PrepareKnowledgebaseCategoryModel();
            await model.PrepareACLModel(null, false, _customerService);
            await model.PrepareStoresMappingModel(null, _storeService, false);
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var knowledgebaseCategory = await _knowledgebaseViewModelService.InsertKnowledgebaseCategoryModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Added"));
                return continueEditing ? RedirectToAction("EditCategory", new { knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);
            //ACL
            await model.PrepareACLModel(null, true, _customerService);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> EditCategory(string id)
        {
            var knowledgebaseCategory = await _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            var model = knowledgebaseCategory.ToModel();
            await _knowledgebaseViewModelService.PrepareCategory(model);

            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseCategory.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = knowledgebaseCategory.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaDescription = knowledgebaseCategory.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseCategory.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseCategory.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseCategory.GetSeName(languageId, false, false);
            });

            await model.PrepareACLModel(knowledgebaseCategory, false, _customerService);
            model.SelectedCustomerRoleIds = knowledgebaseCategory.CustomerRoles.ToArray();

            //Stores
            await model.PrepareStoresMappingModel(knowledgebaseCategory, _storeService, false);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            var knowledgebaseCategory = await _knowledgebaseService.GetKnowledgebaseCategory(model.Id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseCategory = await _knowledgebaseViewModelService.UpdateKnowledgebaseCategoryModel(knowledgebaseCategory, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Updated"));
                return continueEditing ? RedirectToAction("EditCategory", new { id = knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            await _knowledgebaseViewModelService.PrepareCategory(model);
            //Stores
            await model.PrepareStoresMappingModel(knowledgebaseCategory, _storeService, true);
            //ACL
            await model.PrepareACLModel(knowledgebaseCategory, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var knowledgebaseCategory = await _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if ((await _knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(id)).Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Cannotdeletewitharticles"));
                return RedirectToAction("EditCategory", new { id });
            }

            if (ModelState.IsValid)
            {
                await _knowledgebaseViewModelService.DeleteKnowledgebaseCategoryModel(knowledgebaseCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("EditCategory", new { id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> CreateArticle(string parentCategoryId)
        {
            var model = await _knowledgebaseViewModelService.PrepareKnowledgebaseArticleModel();
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);

            if (!string.IsNullOrEmpty(parentCategoryId))
                model.ParentCategoryId = parentCategoryId;

            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CreateArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var knowledgebaseArticle = await _knowledgebaseViewModelService.InsertKnowledgebaseArticleModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Added"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            await _knowledgebaseViewModelService.PrepareCategory(model);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);
            //ACL
            await model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> EditArticle(string id)
        {
            var knowledgebaseArticle = await _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            var model = knowledgebaseArticle.ToModel();
            await _knowledgebaseViewModelService.PrepareCategory(model);
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseArticle.GetLocalized(x => x.Name, languageId, false, false);
                locale.Content = knowledgebaseArticle.GetLocalized(x => x.Content, languageId, false, false);
                locale.MetaDescription = knowledgebaseArticle.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseArticle.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseArticle.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseArticle.GetSeName(languageId, false, false);
            });

            await model.PrepareACLModel(knowledgebaseArticle, false, _customerService);
            model.SelectedCustomerRoleIds = knowledgebaseArticle.CustomerRoles.ToArray();

            //Stores
            await model.PrepareStoresMappingModel(knowledgebaseArticle, _storeService, false);

            model.AllowComments = knowledgebaseArticle.AllowComments;

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            var knowledgebaseArticle = await _knowledgebaseService.GetKnowledgebaseArticle(model.Id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseArticle = await _knowledgebaseViewModelService.UpdateKnowledgebaseArticleModel(knowledgebaseArticle, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Updated"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            await _knowledgebaseViewModelService.PrepareCategory(model);
            //Store
            await model.PrepareStoresMappingModel(knowledgebaseArticle, _storeService, true);
            //ACL
            await model.PrepareACLModel(knowledgebaseArticle, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            var knowledgebaseArticle = await _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _knowledgebaseViewModelService.DeleteKnowledgebaseArticle(knowledgebaseArticle);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("EditArticle", new { knowledgebaseArticle.Id });
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ArticlesPopup(string articleId)
        {
            var model = new KnowledgebaseArticleModel.AddRelatedArticleModel
            {
                ArticleId = articleId
            };
            model.AvailableArticles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var articles = await _knowledgebaseService.GetKnowledgebaseArticles();
            foreach (var a in articles)
                model.AvailableArticles.Add(new SelectListItem { Text = a.Name, Value = a.Id.ToString() });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> RelatedArticlesAddPopupList(DataSourceRequest command, KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            var articles = await _knowledgebaseService.GetKnowledgebaseArticlesByName(model.SearchArticleName, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = articles.Select(x => x.ToModel()),
                Total = articles.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> RelatedArticlesList(DataSourceRequest command, string articleId)
        {
            var articles = await _knowledgebaseService.GetRelatedKnowledgebaseArticles(articleId, command.Page - 1, command.PageSize);
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

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ArticlesPopup(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            if (model.SelectedArticlesIds != null)
            {
                await _knowledgebaseViewModelService.InsertKnowledgebaseRelatedArticle(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> RelatedArticleDelete(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            if (model.ArticleId == null || model.Id == null)
                throw new ArgumentNullException("Article id expected ");

            await _knowledgebaseViewModelService.DeleteKnowledgebaseRelatedArticle(model);
            return new NullJsonResult();
        }
    }
}
