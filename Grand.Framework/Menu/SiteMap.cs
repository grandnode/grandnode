using Grand.Domain.Admin;
using Grand.Services.Admin;
using Grand.Services.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Menu
{
    public class SiteMap
    {
        private readonly IAdminSiteMapService _adminSiteMapService;
        private readonly IPermissionService _permissionService;
        public SiteMap(
            IAdminSiteMapService adminSiteMapService,
            IPermissionService permissionService)
        {
            _adminSiteMapService = adminSiteMapService;
            _permissionService = permissionService;
        }

        public SiteMapNode RootNode { get; set; }

        public virtual async Task Load()
        {
            var adminSiteMaps = await _adminSiteMapService.GetSiteMap();
            var sitemap = new SiteMapNode() {
                SystemName = "Home",
                ResourceName = "Admin.Home",
                ControllerName = "Home",
                ActionName = "Overview",
            };
            await PrepareRootNode(sitemap, adminSiteMaps);
        }


        private async Task PrepareRootNode(SiteMapNode siteMap, IList<AdminSiteMap> adminSiteMaps)
        {
            if (adminSiteMaps != null)
            {
                foreach (var item in adminSiteMaps)
                {
                    var mainsite = new SiteMapNode();
                    siteMap.ChildNodes.Add(mainsite);
                    await Iterate(mainsite, item);
                }
            }
            RootNode = siteMap;
        }

        private async Task Iterate(SiteMapNode siteMap, AdminSiteMap siteMapNode)
        {
            await PopulateNode(siteMap, siteMapNode);

            foreach (var item in siteMapNode.ChildNodes)
            {
                var mainsite = new SiteMapNode();
                siteMap.ChildNodes.Add(mainsite);
                await Iterate(mainsite, item);
            }
        }

        private async Task PopulateNode(SiteMapNode siteMap, AdminSiteMap siteNode)
        {
            siteMap.ActionName = siteNode.ActionName;
            siteMap.AllPermissions = siteNode.AllPermissions;
            siteMap.ControllerName = siteNode.ControllerName;
            siteMap.IconClass = siteNode.IconClass;
            siteMap.OpenUrlInNewTab = siteNode.OpenUrlInNewTab;
            siteMap.ResourceName = siteNode.ResourceName;
            siteMap.SystemName = siteNode.SystemName;
            siteMap.Url = siteNode.Url;

            if (siteNode.PermissionNames.Any())
            {
                if (siteNode.AllPermissions)
                {
                    siteMap.Visible = true;
                    foreach (var permissionName in siteNode.PermissionNames)
                    {
                        if (!await _permissionService.Authorize(permissionName.Trim()))
                            siteMap.Visible = false;
                    }
                }
                else
                {
                    siteMap.Visible = false;
                    foreach (var permissionName in siteNode.PermissionNames)
                    {
                        if (await _permissionService.Authorize(permissionName.Trim()))
                            siteMap.Visible = true;
                    }
                }
            }
            else
            {
                siteMap.Visible = true;
            }
        }
    }
}
