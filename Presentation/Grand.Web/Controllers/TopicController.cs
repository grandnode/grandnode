﻿using System;
using System.Linq;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Topics;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Framework.Security;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Topics;

namespace Grand.Web.Controllers
{
    public partial class TopicController : BasePublicController
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly ITopicTemplateService _topicTemplateService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Constructors

        public TopicController(ITopicService topicService,
            ILocalizationService localizationService,
            IWorkContext workContext, 
            IStoreContext storeContext,
            ICacheManager cacheManager,
            IStoreMappingService storeMappingService,
            ITopicTemplateService topicTemplateService,
            IAclService aclService,
            IPermissionService permissionService)
        {
            this._topicService = topicService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._cacheManager = cacheManager;
            this._storeMappingService = storeMappingService;
            this._topicTemplateService = topicTemplateService;
            this._aclService = aclService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual TopicModel PrepareTopicModel(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            var model = new TopicModel
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IncludeInSitemap = topic.IncludeInSitemap,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Title),
                Body = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Body),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle),
                SeName = topic.GetSeName(),
                TopicTemplateId = topic.TopicTemplateId
            };
            return model;
        }

        #endregion

        #region Methods

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult TopicDetails(string topicId)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_ID_KEY,
                topicId,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                var topic = _topicService.GetTopicById(topicId);
                if (topic == null)
                    return null;
                //Store mapping
                if (!_storeMappingService.Authorize(topic))
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;

                return PrepareTopicModel(topic);
            }
            );

            if (cacheModel == null)
                return RedirectToRoute("HomePage");

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TEMPLATE_MODEL_KEY, cacheModel.TopicTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _topicTemplateService.GetTopicTemplateById(cacheModel.TopicTemplateId);
                if (template == null)
                    template = _topicTemplateService.GetAllTopicTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageTopics))
                DisplayEditLink(Url.Action("Edit", "Topic", new { id = cacheModel.Id, area = "Admin" }));

            return View(templateViewPath, cacheModel);
        }

        public ActionResult TopicDetailsPopup(string systemName)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_SYSTEMNAME_KEY,
                systemName,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));

            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                //load by store
                var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                if (topic == null)
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;
                return PrepareTopicModel(topic);
            });

            if (cacheModel == null)
                return RedirectToRoute("HomePage");

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TEMPLATE_MODEL_KEY, cacheModel.TopicTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _topicTemplateService.GetTopicTemplateById(cacheModel.TopicTemplateId);
                if (template == null)
                    template = _topicTemplateService.GetAllTopicTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            ViewBag.IsPopup = true;
            return View(templateViewPath, cacheModel);
        }

        [ChildActionOnly]
        public ActionResult TopicBlock(string systemName)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_SYSTEMNAME_KEY,
                            systemName,
                            _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                            string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cacheModel = _cacheManager.Get(cacheKey, () =>
            {
                //load by store
                var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                if (topic == null)
                    return null;
                //Store mapping
                if (!_storeMappingService.Authorize(topic))
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;
                return PrepareTopicModel(topic);
            });

            if (cacheModel == null)
                return Content("");

            return PartialView(cacheModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Authenticate(string id, string password)
        {
            var authResult = false;
            var title = string.Empty;
            var body = string.Empty;
            var error = string.Empty;

            var topic = _topicService.GetTopicById(id);

            if (topic != null &&
                //password protected?
                topic.IsPasswordProtected &&
                //store mapping
                _storeMappingService.Authorize(topic) &&
                //ACL (access control list)
                _aclService.Authorize(topic))
            {
                if (topic.Password != null && topic.Password.Equals(password))
                {
                    authResult = true;
                    title = topic.GetLocalized(x => x.Title);
                    body = topic.GetLocalized(x => x.Body);
                }
                else
                {
                    error = _localizationService.GetResource("Topic.WrongPassword");
                }
            }
            return Json(new { Authenticated = authResult, Title = title, Body = body, Error = error });
        }

        #endregion
    }
}
