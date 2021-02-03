using Grand.Core.Caching;
using Grand.Domain.Admin;
using Grand.Domain.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Admin
{
    public class AdminSiteMapService : IAdminSiteMapService
    {
        private readonly IRepository<AdminSiteMap> _adminSiteMapRepository;
        private readonly ICacheManager _cacheManager;

        public AdminSiteMapService(
            IRepository<AdminSiteMap> adminSiteMapRepository,
            ICacheManager cacheManager)
        {
            _adminSiteMapRepository = adminSiteMapRepository;
            _cacheManager = cacheManager;
        }

        public virtual async Task<IList<AdminSiteMap>> GetSiteMap()
        {
            return await _cacheManager.GetAsync($"ADMIN_SITEMAP", async () =>
            {
                var query = from c in _adminSiteMapRepository.Table
                            select c;

                var list = await query.ToListAsync();
                if (list.Any())
                    return list;
                else
                    return StandardAdminSiteMap.SiteMap;
            });
        }
    }
}
