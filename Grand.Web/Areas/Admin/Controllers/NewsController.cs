using Grand.Domain.News;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.News;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.News;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using Grand.Services.Helpers;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.News)]
    public partial class NewsController : BaseAdminController
	{
        #region Fields
        private readonly INewsViewModelService _newsViewModelService;
        private readonly INewsService _newsService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Constructors

        public NewsController(
            INewsViewModelService newsViewModelService,
            INewsService newsService, 
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService, 
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper)
        {
            _newsViewModelService = newsViewModelService;
            _newsService = newsService;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeService = storeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region News items

        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List()
        {
            var model = new NewsItemListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, NewsItemListModel model)
        {
            var news = await _newsViewModelService.PrepareNewsItemModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = news.newsItemModels.ToList(),
                Total = news.totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);

            //default values
            model.Published = true;
            model.AllowComments = true;
            //locales
            await AddLocales(_languageService, model.Locales);
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(NewsItemModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var newsItem = await _newsViewModelService.InsertNewsItemModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, true);
            //ACL
            await model.PrepareACLModel(null, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var newsItem = await _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = newsItem.ToModel(_dateTimeHelper);
            //Store
            await model.PrepareStoresMappingModel(newsItem, _storeService, false);
            //ACL
            await model.PrepareACLModel(newsItem, false, _customerService);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = newsItem.GetLocalized(x => x.Title, languageId, false, false);
                locale.Short = newsItem.GetLocalized(x => x.Short, languageId, false, false);
                locale.Full = newsItem.GetLocalized(x => x.Full, languageId, false, false);
                locale.MetaKeywords = newsItem.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = newsItem.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = newsItem.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = newsItem.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(NewsItemModel model, bool continueEditing)
        {
            var newsItem = await _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = await _newsViewModelService.UpdateNewsItemModel(newsItem, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = newsItem.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            //Store
            await model.PrepareStoresMappingModel(newsItem, _storeService, true);
            //ACL
            await model.PrepareACLModel(newsItem, true, _customerService);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var newsItem = await _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                await _newsService.DeleteNews(newsItem);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = newsItem.Id });
        }

        #endregion

        #region Comments

        public IActionResult Comments(string filterByNewsItemId)
        {
            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            return View();
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> Comments(string filterByNewsItemId, DataSourceRequest command)
        {
            var comments = await _newsViewModelService.PrepareNewsCommentModel(filterByNewsItemId, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = comments.newsCommentModels.ToList(),
                Total = comments.totalCount,
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> CommentDelete(NewsComment model)
        {
            if(ModelState.IsValid)
            {
                await _newsViewModelService.CommentDelete(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}
