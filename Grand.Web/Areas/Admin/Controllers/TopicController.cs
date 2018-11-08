using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Topics;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Topics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class TopicController : BaseAdminController
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ITopicTemplateService _topicTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IAclService _aclService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion Fields

        #region Constructors

        public TopicController(ITopicService topicService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            ITopicTemplateService topicTemplateService,
            ICustomerService customerService,
            IAclService aclService,
            ICustomerActivityService customerActivityService)
        {
            this._topicService = topicService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._urlRecordService = urlRecordService;
            this._topicTemplateService = topicTemplateService;
            this._customerService = customerService;
            this._aclService = aclService;
            this._customerActivityService = customerActivityService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareTemplatesModel(TopicModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _topicTemplateService.GetAllTopicTemplates();
            foreach (var template in templates)
            {
                model.AvailableTopicTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

        #endregion

        #region List

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var model = new TopicListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, TopicListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topicModels = _topicService.GetAllTopics(model.SearchStoreId, true)
                .Select(x => x.ToModel())
                .ToList();

            if (!string.IsNullOrEmpty(model.Name))
            {
                topicModels = topicModels.Where
                    (x => x.SystemName.ToLowerInvariant().Contains(model.Name.ToLowerInvariant()) ||
                    x.Title.ToLowerInvariant().Contains(model.Name.ToLowerInvariant())).ToList();
            }

            //little hack here:
            //we don't have paging supported for topic list page
            //now ensure that topic bodies are not returned. otherwise, we can get the following error:
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var model = new TopicModel();
            //templates
            PrepareTemplatesModel(model);
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (!model.IsPasswordProtected)
                {
                    model.Password = null;
                }

                var topic = model.ToEntity();
                topic.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                topic.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _topicService.InsertTopic(topic);
                //search engine name
                model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
                topic.Locales = model.Locales.ToLocalizedProperty(topic, x => x.Title, _urlRecordService);
                topic.SeName = model.SeName;
                _topicService.UpdateTopic(topic);
                _urlRecordService.SaveSlug(topic, model.SeName, "");
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Added"));

                //activity log
                _customerActivityService.InsertActivity("AddNewTopic", topic.Id, _localizationService.GetResource("ActivityLog.AddNewTopic"), topic.Title ?? topic.SystemName);

                return continueEditing ? RedirectToAction("Edit", new { id = topic.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //templates
            PrepareTemplatesModel(model);
            //Stores
            model.PrepareStoresMappingModel(null, true, _storeService);
            //ACL
            model.PrepareACLModel(null, true, _customerService);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            var model = topic.ToModel();
            model.Url = Url.RouteUrl("Topic", new { SeName = topic.GetSeName() }, "http");
            //templates
            PrepareTemplatesModel(model);
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(model.Id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            if (ModelState.IsValid)
            {
                topic = model.ToEntity(topic);
                topic.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                topic.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                topic.Locales = model.Locales.ToLocalizedProperty(topic, x => x.Title, _urlRecordService);
                model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
                topic.SeName = model.SeName;
                _topicService.UpdateTopic(topic);

                //search engine name
                _urlRecordService.SaveSlug(topic, model.SeName, "");

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Updated"));

                //activity log
                _customerActivityService.InsertActivity("EditTopic", topic.Id, _localizationService.GetResource("ActivityLog.EditTopic"), topic.Title ?? topic.SystemName);

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
            PrepareTemplatesModel(model);
            //Store
            model.PrepareStoresMappingModel(topic, true, _storeService);
            //ACL
            model.PrepareACLModel(topic, true, _customerService);
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                return AccessDeniedView();

            var topic = _topicService.GetTopicById(id);
            if (topic == null)
                //No topic found with the specified id
                return RedirectToAction("List");

            _topicService.DeleteTopic(topic);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Topics.Deleted"));
            //activity log
            _customerActivityService.InsertActivity("DeleteTopic", topic.Id, _localizationService.GetResource("ActivityLog.DeleteTopic"), topic.Title ?? topic.SystemName);

            return RedirectToAction("List");
        }

        #endregion
    }
}
