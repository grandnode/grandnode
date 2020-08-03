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
        private readonly ICacheManager _cacheManager;
        private readonly ICategoryTemplateService _categoryTemplateService;

        public GetCategoryTemplateViewPathHandler(ICacheManager cacheManager, 
            ICategoryTemplateService categoryTemplateService)
        {
            _cacheManager = cacheManager;
            _categoryTemplateService = categoryTemplateService;
        }

        public async Task<string> Handle(GetCategoryTemplateViewPath request, CancellationToken cancellationToken)
        {
            var templateCacheKey = string.Format(ModelCacheEventConst.CATEGORY_TEMPLATE_MODEL_KEY, request.TemplateId);
            var templateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
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
