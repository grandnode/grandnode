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
    public class GetManufacturerTemplateViewPathHandler : IRequestHandler<GetManufacturerTemplateViewPath, string>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;

        public GetManufacturerTemplateViewPathHandler(ICacheManager cacheManager, 
            IManufacturerTemplateService manufacturerTemplateService)
        {
            _cacheManager = cacheManager;
            _manufacturerTemplateService = manufacturerTemplateService;
        }

        public async Task<string> Handle(GetManufacturerTemplateViewPath request, CancellationToken cancellationToken)
        {
            var templateCacheKey = string.Format(ModelCacheEventConst.MANUFACTURER_TEMPLATE_MODEL_KEY, request.TemplateId);
            var templateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
            {
                var template = await _manufacturerTemplateService.GetManufacturerTemplateById(request.TemplateId);
                if (template == null)
                    template = (await _manufacturerTemplateService.GetAllManufacturerTemplates()).FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });
            return templateViewPath;
        }
    }
}
