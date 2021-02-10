using Grand.Core.Caching;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetCategoryTemplateViewPathHandler : IRequestHandler<GetCategoryTemplateViewPath, string>
    {
        private readonly ICacheBase _cacheBase;
        private readonly ICategoryTemplateService _categoryTemplateService;

        public GetCategoryTemplateViewPathHandler(ICacheBase cacheManager, 
            ICategoryTemplateService categoryTemplateService)
        {
            _cacheBase = cacheManager;
            _categoryTemplateService = categoryTemplateService;
        }

        public async Task<string> Handle(GetCategoryTemplateViewPath request, CancellationToken cancellationToken)
        {
            var templateCacheKey = string.Format(ModelCacheEventConst.CATEGORY_TEMPLATE_MODEL_KEY, request.TemplateId);
            var templateViewPath = await _cacheBase.GetAsync(templateCacheKey, async () =>
            {
                var template = await _categoryTemplateService.GetCategoryTemplateById(request.TemplateId);
                if (template == null)
                    template = (await _categoryTemplateService.GetAllCategoryTemplates()).FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return templateViewPath;
        }
    }
}
