using Grand.Core.Domain.Topics;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Topics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class TopicViewModelService : ITopicViewModelService
    {
        private readonly ITopicTemplateService _topicTemplateService;
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreService _storeService;
        public TopicViewModelService(ITopicTemplateService topicTemplateService, ITopicService topicService, IUrlRecordService urlRecordService, ILocalizationService localizationService,
            ICustomerActivityService customerActivityService, IStoreService storeService)
        {
            _topicTemplateService = topicTemplateService;
            _topicService = topicService;
            _urlRecordService = urlRecordService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _storeService = storeService;
        }

        public virtual TopicListModel PrepareTopicListModel()
        {
            var model = new TopicListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            return model;
        }

        public virtual void PrepareTemplatesModel(TopicModel model)
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
        public virtual Topic InsertTopicModel(TopicModel model)
        {
            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            var topic = model.ToEntity();
            _topicService.InsertTopic(topic);
            //search engine name
            model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
            topic.Locales = model.Locales.ToLocalizedProperty(topic, x => x.Title, _urlRecordService);
            topic.SeName = model.SeName;
            _topicService.UpdateTopic(topic);
            _urlRecordService.SaveSlug(topic, model.SeName, "");

            //activity log
            _customerActivityService.InsertActivity("AddNewTopic", topic.Id, _localizationService.GetResource("ActivityLog.AddNewTopic"), topic.Title ?? topic.SystemName);
            return topic;
        }
        public virtual Topic UpdateTopicModel(Topic topic, TopicModel model)
        {
            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }
            topic = model.ToEntity(topic);
            topic.Locales = model.Locales.ToLocalizedProperty(topic, x => x.Title, _urlRecordService);
            model.SeName = topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true);
            topic.SeName = model.SeName;
            _topicService.UpdateTopic(topic);

            //search engine name
            _urlRecordService.SaveSlug(topic, model.SeName, "");
            //activity log
            _customerActivityService.InsertActivity("EditTopic", topic.Id, _localizationService.GetResource("ActivityLog.EditTopic"), topic.Title ?? topic.SystemName);

            return topic;
        }
        public virtual void DeleteTopic(Topic topic)
        {
            _topicService.DeleteTopic(topic);
            //activity log
            _customerActivityService.InsertActivity("DeleteTopic", topic.Id, _localizationService.GetResource("ActivityLog.DeleteTopic"), topic.Title ?? topic.SystemName);

        }
    }
}
