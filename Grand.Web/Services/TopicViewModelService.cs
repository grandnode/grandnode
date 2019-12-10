using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Topics;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Topics;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Topics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class TopicViewModelService: ITopicViewModelService
    {
        private readonly ITopicService _topicService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly ITopicTemplateService _topicTemplateService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;

        public TopicViewModelService(ITopicService topicService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICacheManager cacheManager,
            ITopicTemplateService topicTemplateService,
            IStoreMappingService storeMappingService,
            IAclService aclService)
        {
            _topicService = topicService;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _topicTemplateService = topicTemplateService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
        }
        public virtual TopicModel PrepareTopicModel(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            var model = new TopicModel
            {
                Id = topic.Id,
                SystemName = topic.SystemName,
                IncludeInSitemap = topic.IncludeInSitemap,
                IsPasswordProtected = topic.IsPasswordProtected,
                Title = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id),
                Body = topic.IsPasswordProtected ? "" : topic.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id),
                MetaKeywords = topic.GetLocalized(x => x.MetaKeywords, _workContext.WorkingLanguage.Id),
                MetaDescription = topic.GetLocalized(x => x.MetaDescription, _workContext.WorkingLanguage.Id),
                MetaTitle = topic.GetLocalized(x => x.MetaTitle, _workContext.WorkingLanguage.Id),
                SeName = topic.GetSeName(_workContext.WorkingLanguage.Id),
                TopicTemplateId = topic.TopicTemplateId,
                Published = topic.Published
            };
            return model;

        }
        public virtual async Task<TopicModel> TopicDetails(string topicId)
        {

            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_ID_KEY,
                topicId,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var topic = await _topicService.GetTopicById(topicId);
                    if (topic == null)
                        return null;
                    //Store mapping
                    if (! _storeMappingService.Authorize(topic))
                        return null;
                    //ACL (access control list)
                    if (!_aclService.Authorize(topic))
                        return null;

                    return PrepareTopicModel(topic);
                }
            );

            return cacheModel;

        }
        public virtual async Task<TopicModel> TopicDetailsPopup(string systemName)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_SYSTEMNAME_KEY,
                systemName,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));

            var cacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                //load by store
                var topic = await _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                if (topic == null)
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;
                return PrepareTopicModel(topic);
            });
            return cacheModel;
        }
        public virtual async Task<TopicModel> TopicBlock(string systemName)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_BY_SYSTEMNAME_KEY,
                systemName,
                _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()));
            var cacheModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                //load by store
                var topic = await _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
                if (topic == null || !topic.Published)
                    return null;
                //Store mapping
                if (!_storeMappingService.Authorize(topic))
                    return null;
                //ACL (access control list)
                if (!_aclService.Authorize(topic))
                    return null;
                return PrepareTopicModel(topic);
            });

            return cacheModel;
        }
        public virtual async Task<string> PrepareTopicTemplateViewPath(string templateId)
        {
            var templateCacheKey = string.Format(ModelCacheEventConsumer.TOPIC_TEMPLATE_MODEL_KEY, templateId);
            var templateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
            {
                var template = await _topicTemplateService.GetTopicTemplateById(templateId);
                if (template == null)
                    template = (await _topicTemplateService.GetAllTopicTemplates()).FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });
            return templateViewPath;
        }
    }
}