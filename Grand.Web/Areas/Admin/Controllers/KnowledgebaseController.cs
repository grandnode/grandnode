using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Core.Domain.Localization;
using Grand.Services.Logging;
using Grand.Services.Customers;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Helpers;
using Grand.Services.Seo;
using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Services.Stores;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class KnowledgebaseController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;

        public KnowledgebaseController(ILocalizationService localizationService, IPermissionService permissionService,
            IKnowledgebaseService knowledgebaseService, ILanguageService languageService, ICustomerActivityService customerActivityService,
            ICustomerService customerService, IDateTimeHelper dateTimeHelper, IUrlRecordService urlRecordService, IStoreService storeService)
        {
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._knowledgebaseService = knowledgebaseService;
            this._languageService = languageService;
            this._customerActivityService = customerActivityService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(KnowledgebaseCategory category, KnowledgebaseCategoryModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = category.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(category, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });

                if (!(String.IsNullOrEmpty(local.Description)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Description",
                        LocaleValue = local.Description,
                    });

                if (!(String.IsNullOrEmpty(local.MetaDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaDescription",
                        LocaleValue = local.MetaDescription,
                    });

                if (!(String.IsNullOrEmpty(local.MetaKeywords)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaKeywords",
                        LocaleValue = local.MetaKeywords,
                    });

                if (!(String.IsNullOrEmpty(local.MetaTitle)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaTitle",
                        LocaleValue = local.MetaTitle,
                    });

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName,
                    });
            }

            return localized;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(KnowledgebaseArticle article, KnowledgebaseArticleModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = article.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(article, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });

                if (!(String.IsNullOrEmpty(local.Content)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Content",
                        LocaleValue = local.Content,
                    });

                if (!(String.IsNullOrEmpty(local.MetaDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaDescription",
                        LocaleValue = local.MetaDescription,
                    });

                if (!(String.IsNullOrEmpty(local.MetaKeywords)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaKeywords",
                        LocaleValue = local.MetaKeywords,
                    });

                if (!(String.IsNullOrEmpty(local.MetaTitle)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaTitle",
                        LocaleValue = local.MetaTitle,
                    });

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName,
                    });
            }

            return localized;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            return View();
        }

        public IActionResult NodeList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

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

            return Json(nodeList);
        }

        public void FillChildNodes(TreeNode parentNode, List<ITreeNode> nodes)
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

        public IActionResult ArticleList(DataSourceRequest command, string parentCategoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var articles = _knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(parentCategoryId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = articles.Select(x => new KnowledgebaseArticleGridModel
                {
                    Name = x.Name,
                    DisplayOrder = x.DisplayOrder,
                    Published = x.Published,
                    ArticleId = x.Id,
                    Id = x.Id
                }),
                Total = articles.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListCategoryActivityLog(DataSourceRequest command, string categoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return Content("");

            var activityLog = _customerActivityService.GetKnowledgebaseCategoryActivities(null, null, categoryId, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
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
                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ListArticleActivityLog(DataSourceRequest command, string articleId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return Content("");

            var activityLog = _customerActivityService.GetKnowledgebaseArticleActivities(null, null, articleId, command.Page - 1, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
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
                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        public IActionResult CreateCategory()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var model = new KnowledgebaseCategoryModel();
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

            model.AvailableCustomerRoles = _customerService
            .GetAllCustomerRoles(true)
            .Select(cr => cr.ToModel())
            .ToList();
            model.Published = true;
            model.AvailableStores = _storeService
            .GetAllStores()
            .Select(s => s.ToModel())
            .ToList();

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var knowledgebaseCategory = model.ToEntity();
                knowledgebaseCategory.CreatedOnUtc = DateTime.UtcNow;
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                knowledgebaseCategory.Locales = UpdateLocales(knowledgebaseCategory, model);
                knowledgebaseCategory.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                model.SeName = knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true);
                knowledgebaseCategory.SeName = model.SeName;
                knowledgebaseCategory.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _knowledgebaseService.InsertKnowledgebaseCategory(knowledgebaseCategory);

                _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");

                _customerActivityService.InsertActivity("CreateKnowledgebaseCategory", knowledgebaseCategory.Id,
                    _localizationService.GetResource("ActivityLog.CreateKnowledgebaseCategory"), knowledgebaseCategory.Name);

                model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
                model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Added"));
                return continueEditing ? RedirectToAction("EditCategory", new { knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            var model = knowledgebaseCategory.ToModel();
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

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseCategory.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = knowledgebaseCategory.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaDescription = knowledgebaseCategory.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseCategory.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseCategory.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseCategory.GetSeName(languageId, false, false);
            });

            model.AvailableCustomerRoles = _customerService
            .GetAllCustomerRoles(true)
            .Select(cr => cr.ToModel())
            .ToList();
            model.SelectedCustomerRoleIds = knowledgebaseCategory.CustomerRoles.ToArray();
            model.AvailableStores = _storeService
            .GetAllStores()
            .Select(s => s.ToModel())
            .ToList();
            model.SelectedStoreIds = knowledgebaseCategory.Stores.ToArray();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditCategory(KnowledgebaseCategoryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(model.Id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseCategory = model.ToEntity(knowledgebaseCategory);
                knowledgebaseCategory.UpdatedOnUtc = DateTime.UtcNow;
                knowledgebaseCategory.Locales = UpdateLocales(knowledgebaseCategory, model);
                knowledgebaseCategory.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                model.SeName = knowledgebaseCategory.ValidateSeName(model.SeName, knowledgebaseCategory.Name, true);
                knowledgebaseCategory.SeName = model.SeName;
                knowledgebaseCategory.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _knowledgebaseService.UpdateKnowledgebaseCategory(knowledgebaseCategory);

                _urlRecordService.SaveSlug(knowledgebaseCategory, model.SeName, "");

                _customerActivityService.InsertActivity("UpdateKnowledgebaseCategory", knowledgebaseCategory.Id,
                    _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseCategory"), knowledgebaseCategory.Name);

                model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
                model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Updated"));
                return continueEditing ? RedirectToAction("EditCategory", new { id = knowledgebaseCategory.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteCategory(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseCategory = _knowledgebaseService.GetKnowledgebaseCategory(id);
            if (knowledgebaseCategory == null)
                return RedirectToAction("List");

            if (_knowledgebaseService.GetKnowledgebaseArticlesByCategoryId(id).Any())
            {
                ErrorNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Cannotdeletewitharticles"));
                return RedirectToAction("EditCategory", new { id });
            }

            _knowledgebaseService.DeleteKnowledgebaseCategory(knowledgebaseCategory);

            _customerActivityService.InsertActivity("DeleteKnowledgebaseCategory", knowledgebaseCategory.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseCategory"), knowledgebaseCategory.Name);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseCategory.Deleted"));
            return RedirectToAction("List");
        }

        public IActionResult CreateArticle(string parentCategoryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var model = new KnowledgebaseArticleModel();
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

            model.AvailableCustomerRoles = _customerService
            .GetAllCustomerRoles(true)
            .Select(cr => cr.ToModel())
            .ToList();
            model.Published = true;
            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();

            if (!string.IsNullOrEmpty(parentCategoryId))
                model.ParentCategoryId = parentCategoryId;

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult CreateArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var knowledgebaseArticle = model.ToEntity();
                knowledgebaseArticle.CreatedOnUtc = DateTime.UtcNow;
                knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
                knowledgebaseArticle.Locales = UpdateLocales(knowledgebaseArticle, model);
                knowledgebaseArticle.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                model.SeName = knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true);
                knowledgebaseArticle.SeName = model.SeName;
                knowledgebaseArticle.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _knowledgebaseService.InsertKnowledgebaseArticle(knowledgebaseArticle);

                _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");

                _customerActivityService.InsertActivity("CreateKnowledgebaseArticle", knowledgebaseArticle.Id,
                    _localizationService.GetResource("ActivityLog.CreateKnowledgebaseArticle"), knowledgebaseArticle.Name);

                model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
                model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Added"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult EditArticle(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            var model = knowledgebaseArticle.ToModel();
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

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = knowledgebaseArticle.GetLocalized(x => x.Name, languageId, false, false);
                locale.Content = knowledgebaseArticle.GetLocalized(x => x.Content, languageId, false, false);
                locale.MetaDescription = knowledgebaseArticle.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaKeywords = knowledgebaseArticle.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaTitle = knowledgebaseArticle.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = knowledgebaseArticle.GetSeName(languageId, false, false);
            });

            model.AvailableCustomerRoles = _customerService
            .GetAllCustomerRoles(true)
            .Select(cr => cr.ToModel())
            .ToList();
            model.SelectedCustomerRoleIds = knowledgebaseArticle.CustomerRoles.ToArray();
            model.AvailableStores = _storeService
            .GetAllStores()
            .Select(s => s.ToModel())
            .ToList();
            model.SelectedStoreIds = knowledgebaseArticle.Stores.ToArray();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditArticle(KnowledgebaseArticleModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(model.Id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                knowledgebaseArticle = model.ToEntity(knowledgebaseArticle);
                knowledgebaseArticle.UpdatedOnUtc = DateTime.UtcNow;
                knowledgebaseArticle.Locales = UpdateLocales(knowledgebaseArticle, model);
                knowledgebaseArticle.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                model.SeName = knowledgebaseArticle.ValidateSeName(model.SeName, knowledgebaseArticle.Name, true);
                knowledgebaseArticle.SeName = model.SeName;
                knowledgebaseArticle.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _knowledgebaseService.UpdateKnowledgebaseArticle(knowledgebaseArticle);

                _urlRecordService.SaveSlug(knowledgebaseArticle, model.SeName, "");

                _customerActivityService.InsertActivity("UpdateKnowledgebaseArticle", knowledgebaseArticle.Id,
                    _localizationService.GetResource("ActivityLog.UpdateKnowledgebaseArticle"), knowledgebaseArticle.Name);

                model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
                model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Updated"));
                return continueEditing ? RedirectToAction("EditArticle", new { knowledgebaseArticle.Id }) : RedirectToAction("EditCategory", new { id = model.ParentCategoryId });
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteArticle(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var knowledgebaseArticle = _knowledgebaseService.GetKnowledgebaseArticle(id);
            if (knowledgebaseArticle == null)
                return RedirectToAction("List");

            _knowledgebaseService.DeleteKnowledgebaseArticle(knowledgebaseArticle);

            _customerActivityService.InsertActivity("DeleteKnowledgebaseArticle", knowledgebaseArticle.Id,
                _localizationService.GetResource("ActivityLog.DeleteKnowledgebaseArticle"), knowledgebaseArticle.Name);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Knowledgebase.KnowledgebaseArticle.Deleted"));
            return RedirectToAction("List");
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            var articles = _knowledgebaseService.GetKnowledgebaseArticlesByName(model.SearchArticleName);
            var gridModel = new DataSourceResult();
            gridModel.Data = articles.Select(x => x.ToModel());
            gridModel.Total = articles.TotalCount;

            return Json(gridModel);
        }

        public IActionResult RelatedArticlesList(DataSourceRequest command, string articleId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageKnowledgebase))
                return AccessDeniedView();

            if (model.SelectedArticlesIds != null)
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

            ViewBag.RefreshPage = true;

            return View(model);
        }

        [HttpPost]
        public IActionResult RelatedArticleDelete(KnowledgebaseArticleModel.AddRelatedArticleModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.ArticleId == null || model.Id == null)
                throw new ArgumentNullException("Article id expected ");

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

            return new NullJsonResult();
        }
    }
}
