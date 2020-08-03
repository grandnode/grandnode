using Grand.Domain.Seo;
using Grand.Domain.Topics;
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
using System.Threading.Tasks;

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
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSettings;

        public TopicViewModelService(ITopicTemplateService topicTemplateService, ITopicService topicService, IUrlRecordService urlRecordService, ILocalizationService localizationService,
            ICustomerActivityService customerActivityService, IStoreService storeService, ILanguageService languageService, SeoSettings seoSettings)
        {
            _topicTemplateService = topicTemplateService;
            _topicService = topicService;
            _urlRecordService = urlRecordService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _storeService = storeService;
            _languageService = languageService;
            _seoSettings = seoSettings;
        }

        public virtual async Task<TopicListModel> PrepareTopicListModel()
        {
            var model = new TopicListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            return model;
        }

        public virtual async Task PrepareTemplatesModel(TopicModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = await _topicTemplateService.GetAllTopicTemplates();
            foreach (var template in templates)
            {
                model.AvailableTopicTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }
        public virtual async Task<Topic> InsertTopicModel(TopicModel model)
        {
            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }

            var topic = model.ToEntity();
            await _topicService.InsertTopic(topic);
            //search engine name
            model.SeName = await topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true, _seoSettings, _urlRecordService, _languageService);
            topic.Locales = await model.Locales.ToLocalizedProperty(topic, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            topic.SeName = model.SeName;
            await _topicService.UpdateTopic(topic);
            await _urlRecordService.SaveSlug(topic, model.SeName, "");

            //activity log
            await _customerActivityService.InsertActivity("AddNewTopic", topic.Id, _localizationService.GetResource("ActivityLog.AddNewTopic"), topic.Title ?? topic.SystemName);
            return topic;
        }
        public virtual async Task<Topic> UpdateTopicModel(Topic topic, TopicModel model)
        {
            if (!model.IsPasswordProtected)
            {
                model.Password = null;
            }
            topic = model.ToEntity(topic);
            topic.Locales = await model.Locales.ToLocalizedProperty(topic, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await topic.ValidateSeName(model.SeName, topic.Title ?? topic.SystemName, true, _seoSettings, _urlRecordService, _languageService);
            topic.SeName = model.SeName;
            await _topicService.UpdateTopic(topic);

            //search engine name
            await _urlRecordService.SaveSlug(topic, model.SeName, "");
            //activity log
            await _customerActivityService.InsertActivity("EditTopic", topic.Id, _localizationService.GetResource("ActivityLog.EditTopic"), topic.Title ?? topic.SystemName);

            return topic;
        }
        public virtual async Task DeleteTopic(Topic topic)
        {
            await _topicService.DeleteTopic(topic);
            //activity log
            await _customerActivityService.InsertActivity("DeleteTopic", topic.Id, _localizationService.GetResource("ActivityLog.DeleteTopic"), topic.Title ?? topic.SystemName);
        }
    }
}
