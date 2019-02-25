using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Topics;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Topics)]
    public partial class TopicController : BaseAdminController
    {
        #region Fields
        private readonly ITopicViewModelService _topicViewModelService;
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly ICustomerService _customerService;
        #endregion Fields

        #region Constructors

        public TopicController(
            ITopicViewModelService topicViewModelService,
            ITopicService topicService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreService storeService,
            ICustomerService customerService)
        {
            this._topicViewModelService = topicViewModelService;
            this._topicService = topicService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._customerService = customerService;
        }

        #endregion

        #region List

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _topicViewModelService.PrepareTopicListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, TopicListModel model)
        {
            var topicModels = _topicService.GetAllTopics(model.SearchStoreId, true)
                .Select(x => x.ToModel())
                .ToList();

            if (!string.IsNullOrEmpty(model.Name))
            {
                topicModels = topicModels.Where
                    (x => x.SystemName.ToLowerInvariant().Contains(model.Name.ToLowerInvariant()) ||
                    x.Title.ToLowerInvariant().Contains(model.Name.ToLowerInvariant())).ToList();
            }
            //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
            foreach (var topic in topicModels)
            {
                topic.Body = "";
            }
            var gridModel = new DataSourceResult
            {
                Data = topicModels,
                Total = topicModels.Count
            };

            return Json(gridModel);
        }

        #endregion

        #region Create / Edit / Delete

        public IActionResult Create()
        {
            var model = new TopicModel();
            //templates
            _topicViewModelService.PrepareTemplatesModel(model);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //locales
            AddLocales(_languageService, model.Locales);
            //ACL
            model.PrepareACLModel(null, false, _customerService);
            //default values
            model.DisplayOrder = 1;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(TopicModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var topic = _topicViewModelService.InsertTopicModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = topic.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            _topicViewModelService.PrepareTemplatesModel(model);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //ACL
            model.PrepareACLModel(null, true, _customerService);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            var model = topic.ToModel();
            model.Url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
            //templates
            _topicViewModelService.PrepareTemplatesModel(model);
            //ACL
            model.PrepareACLModel(topic, false, _customerService);
            //Store
            model.PrepareStoresMappingModel(topic, false, _storeService);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = topic.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = topic.GetLocalized(x => x.Body, languageId, false, false);
                locale.MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = topic.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = topic.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = topic.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(TopicModel model, bool continueEditing)
        {
            var topic = _topicService.GetTopicById(model.Id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                topic = _topicViewModelService.UpdateTopicModel(topic, model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = topic.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.Url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
            //templates
            _topicViewModelService.PrepareTemplatesModel(model);
            //Store
            model.PrepareStoresMappingModel(topic, true, _storeService);
            //ACL
            model.PrepareACLModel(topic, true, _customerService);
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _topicViewModelService.DeleteTopic(topic);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion
    }
}
