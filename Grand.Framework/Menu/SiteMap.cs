using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Security;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Menu
{
    public class SiteMap
    {
        private readonly IPermissionService _permissionService;
        private readonly ICacheManager _cacheManager;
        public SiteMap(IPermissionService permissionService, ICacheManager cacheManager)
        {
            _permissionService = permissionService;
            _cacheManager = cacheManager;
        }

        public SiteMapNode RootNode { get; set; }

        public virtual async Task LoadFrom(string physicalPath)
        {
            var sitemap = _cacheManager.Get($"ADMIN_SITEMAP_{physicalPath}", () =>
            {
                var filePath = CommonHelper.MapPath(physicalPath);
                var content = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(content))
                {
                    return JsonConvert.DeserializeObject<SiteMapNode>(content);
                }
                else
                    return new SiteMapNode();
            });
            await PrepareRootNode(sitemap);
        }

        private async Task PrepareRootNode(SiteMapNode siteMap)
        {
            if (siteMap != null)
            {
                await Iterate(siteMap);
            }
            RootNode = siteMap;
        }

        private async Task Iterate(SiteMapNode siteMapNode)
        {
            await PopulateNode(siteMapNode);

            foreach (var item in siteMapNode.ChildNodes)
            {
                await Iterate(item);
            }
        }

        private async Task PopulateNode(SiteMapNode siteNode)
        {
            if (!string.IsNullOrEmpty(siteNode.ControllerName) && !string.IsNullOrEmpty(siteNode.ActionName))
            {
                siteNode.RouteValues = new RouteValueDictionary { { "area", "Admin" } };
            }
            if (siteNode.PermissionNames.Any())
            {
                if (siteNode.AllPermissions)
                {
                    siteNode.Visible = true;
                    foreach (var permissionName in siteNode.PermissionNames)
                    {
                        if (!await _permissionService.Authorize(permissionName.Trim()))
                            siteNode.Visible = false;
                    }
                }
                else
                {
                    siteNode.Visible = false;
                    foreach (var permissionName in siteNode.PermissionNames)
                    {
                        if (await _permissionService.Authorize(permissionName.Trim()))
                            siteNode.Visible = true;
                    }
                }
            }
            else
            {
                siteNode.Visible = true;
            }
        }
    }
}
