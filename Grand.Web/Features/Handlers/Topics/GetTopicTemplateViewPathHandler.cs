using Grand.Core.Caching;
using Grand.Services.Topics;
using Grand.Web.Features.Models.Topics;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Topics
{
    public class GetTopicTemplateViewPathHandler : IRequestHandler<GetTopicTemplateViewPath, string>
    {
        private readonly ICacheManager _cacheManager;
        private readonly ITopicTemplateService _topicTemplateService;

        public GetTopicTemplateViewPathHandler(ICacheManager cacheManager,
            ITopicTemplateService topicTemplateService)
        {
            _cacheManager = cacheManager;
            _topicTemplateService = topicTemplateService;
        }

        public async Task<string> Handle(GetTopicTemplateViewPath request, CancellationToken cancellationToken)
        {
            var templateCacheKey = string.Format(ModelCacheEventConst.TOPIC_TEMPLATE_MODEL_KEY, request.TemplateId);
            var templateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
            {
                var template = await _topicTemplateService.GetTopicTemplateById(request.TemplateId);
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
