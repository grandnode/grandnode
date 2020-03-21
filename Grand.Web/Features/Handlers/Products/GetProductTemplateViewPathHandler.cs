using Grand.Core.Caching;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductTemplateViewPathHandler : IRequestHandler<GetProductTemplateViewPath, string>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IProductTemplateService _productTemplateService;

        public GetProductTemplateViewPathHandler(ICacheManager cacheManager, IProductTemplateService productTemplateService)
        {
            _cacheManager = cacheManager;
            _productTemplateService = productTemplateService;
        }

        public async Task<string> Handle(GetProductTemplateViewPath request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.ProductTemplateId))
                throw new ArgumentNullException("ProductTemplateId");

            var templateCacheKey = string.Format(ModelCacheEventConst.PRODUCT_TEMPLATE_MODEL_KEY, request.ProductTemplateId);
            var productTemplateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
            {
                var template = await _productTemplateService.GetProductTemplateById(request.ProductTemplateId);
                if (template == null)
                    template = (await _productTemplateService.GetAllProductTemplates()).FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return productTemplateViewPath;
        }
    }
}
