﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.News;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.News;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.News;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;

namespace Grand.Admin.Controllers
{
    public partial class NewsController : BaseAdminController
	{
		#region Fields

        private readonly INewsService _newsService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Constructors

        public NewsController(INewsService newsService, 
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            IStoreService storeService, 
            IStoreMappingService storeMappingService,
            ICustomerService customerService)
        {
            this._newsService = newsService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareAclModel(NewsItemModel model, NewsItem newsItem, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (newsItem != null)
                {
                    model.SelectedCustomerRoleIds = newsItem.CustomerRoles.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(NewsItemModel model, NewsItem newsItem, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (newsItem != null)
                {
                    model.SelectedStoreIds = newsItem.Stores.ToArray();
                }
            }
        }


        #endregion

        #region News items

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var model = new NewsItemListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, NewsItemListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var news = _newsService.GetAllNews("", model.SearchStoreId, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = news.Select(x =>
                {
                    var m = x.ToModel();
                    m.Full = "";
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = _languageService.GetLanguageById(x.LanguageId).Name;
                    m.Comments = x.CommentCount;
                    return m;
                }),
                Total = news.TotalCount
            };

            return Json(gridModel);
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.AllowComments = true;
            //ACL
            PrepareAclModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                newsItem.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                newsItem.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _newsService.InsertNews(newsItem);

                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                newsItem.SeName = seName;
                _newsService.UpdateNews(newsItem);
                //search engine name
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            //ACL
            PrepareAclModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = newsItem.ToModel();
            model.StartDate = newsItem.StartDateUtc;
            model.EndDate = newsItem.EndDateUtc;
            //Store
            PrepareStoresMappingModel(model, newsItem, false);
            //ACL
            PrepareAclModel(model, newsItem, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                newsItem.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                newsItem.SeName = seName;
                _newsService.UpdateNews(newsItem);

                //search engine name
                
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = newsItem.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Store
            PrepareStoresMappingModel(model, newsItem, true);
            //ACL
            PrepareAclModel(model, newsItem, false);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            _newsService.DeleteNews(newsItem);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Comments

        public ActionResult Comments(int? filterByNewsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            return View();
        }

        [HttpPost]
        public ActionResult Comments(string filterByNewsItemId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            IList<NewsComment> comments;
            if (!String.IsNullOrEmpty(filterByNewsItemId))
            {
                //filter comments by news item
                var newsItem = _newsService.GetNewsById(filterByNewsItemId);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = _newsService.GetAllComments("");
            }

            var gridModel = new DataSourceResult
            {
                Data = comments.PagedForCommand(command).Select(newsComment =>
                {
                    var commentModel = new NewsCommentModel();
                    commentModel.Id = newsComment.Id;
                    commentModel.NewsItemId = newsComment.NewsItemId;
                    commentModel.NewsItemTitle = _newsService.GetNewsById(newsComment.NewsItemId).Title;
                    commentModel.CustomerId = newsComment.CustomerId;
                    var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(newsComment.CustomerId);
                    commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.CommentTitle = newsComment.CommentTitle;
                    commentModel.CommentText = Core.Html.HtmlHelper.FormatText(newsComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }),
                Total = comments.Count,
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CommentDelete(NewsComment model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(model.NewsItemId);
            var comment = newsItem.NewsComments.FirstOrDefault(x => x.Id == model.Id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            //var newsItem = _newsService.GetNewsById(comment.NewsItemId);
            //_newsService.(comment);
            newsItem.NewsComments.Remove(comment);
            //update totals
            newsItem.CommentCount = newsItem.NewsComments.Count;
            _newsService.UpdateNews(newsItem);

            return new NullJsonResult();
        }


        #endregion
    }
}
