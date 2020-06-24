using Grand.Core.Caching;
using Grand.Services.Customers;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Common;
using Grand.Web.Infrastructure.Cache;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Common
{
    public class GetSitemapXMLHandler : IRequestHandler<GetSitemapXml, string>
    {
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly ICacheManager _cacheManager;

        public GetSitemapXMLHandler(ISitemapGenerator sitemapGenerator, ICacheManager cacheManager)
        {
            _sitemapGenerator = sitemapGenerator;
            _cacheManager = cacheManager;
        }

        public async Task<string> Handle(GetSitemapXml request, CancellationToken cancellationToken)
        {
            string cacheKey = string.Format(ModelCacheEventConst.SITEMAP_SEO_MODEL_KEY, request.Id,
                request.Language.Id,
                string.Join(",", request.Customer.GetCustomerRoleIds()),
                request.Store.Id);
            var siteMap = await _cacheManager.GetAsync(cacheKey, () => _sitemapGenerator.Generate(request.UrlHelper, request.Id, request.Language.Id, request.Store.Id));
            return siteMap;
        }
    }
}
