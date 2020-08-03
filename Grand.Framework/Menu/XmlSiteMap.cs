using Grand.Core;
using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Grand.Framework.Menu
{
    public class XmlSiteMap
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public XmlSiteMap(ILocalizationService localizationService, IPermissionService permissionService)
        {
            RootNode = new SiteMapNode();
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        public SiteMapNode RootNode { get; set; }

        public virtual async Task LoadFrom(string physicalPath)
        {
            string filePath = CommonHelper.MapPath(physicalPath);
            string content = File.ReadAllText(filePath);

            if (!string.IsNullOrEmpty(content))
            {
                using (var sr = new StringReader(content))
                {
                    using (var xr = XmlReader.Create(sr,
                            new XmlReaderSettings
                            {
                                CloseInput = true,
                                IgnoreWhitespace = true,
                                IgnoreComments = true,
                                IgnoreProcessingInstructions = true
                            }))
                    {
                        var doc = new XmlDocument();
                        doc.Load(xr);

                        if ((doc.DocumentElement != null) && doc.HasChildNodes)
                        {
                            XmlNode xmlRootNode = doc.DocumentElement.FirstChild;
                            await Iterate(RootNode, xmlRootNode);
                        }
                    }
                }
            }
        }

        private async Task Iterate(SiteMapNode siteMapNode, XmlNode xmlNode)
        {
            await PopulateNode(siteMapNode, xmlNode);

            foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
            {
                if (xmlChildNode.LocalName.Equals("siteMapNode", StringComparison.OrdinalIgnoreCase))
                {
                    var siteMapChildNode = new SiteMapNode();
                    siteMapNode.ChildNodes.Add(siteMapChildNode);

                    await Iterate(siteMapChildNode, xmlChildNode);
                }
            }
        }

        private async Task PopulateNode(SiteMapNode siteMapNode, XmlNode xmlNode)
        {
            //system name
            siteMapNode.SystemName = GetStringValueFromAttribute(xmlNode, "SystemName");

            //title
            var resource = GetStringValueFromAttribute(xmlNode, "grandResource");
            siteMapNode.Title = _localizationService.GetResource(resource);

            //routes, url
            string controllerName = GetStringValueFromAttribute(xmlNode, "controller");
            string actionName = GetStringValueFromAttribute(xmlNode, "action");
            string url = GetStringValueFromAttribute(xmlNode, "url");
            if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(actionName))
            {
                siteMapNode.ControllerName = controllerName;
                siteMapNode.ActionName = actionName;

                siteMapNode.RouteValues = new RouteValueDictionary { { "area", "Admin" } };
            }
            else if (!string.IsNullOrEmpty(url))
            {
                siteMapNode.Url = url;
            }

            //image URL
            siteMapNode.IconClass = GetStringValueFromAttribute(xmlNode, "IconClass");

            //permission name
            var permissionNames = GetStringValueFromAttribute(xmlNode, "PermissionNames");
            if (!string.IsNullOrEmpty(permissionNames))
            {
                var fullpermissions = GetStringValueFromAttribute(xmlNode, "AllPermissions");
                if (!string.IsNullOrEmpty(fullpermissions) && fullpermissions == "true")
                {
                    siteMapNode.Visible = true;
                    foreach (var permissionName in permissionNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!await _permissionService.Authorize(permissionName.Trim()))
                            siteMapNode.Visible = false;
                    }
                }
                else
                {
                    siteMapNode.Visible = false;
                    foreach (var permissionName in permissionNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (await _permissionService.Authorize(permissionName.Trim()))
                            siteMapNode.Visible = true;
                    }
                }
            }
            else
            {
                siteMapNode.Visible = true;
            }

            // Open URL in new tab
            var openUrlInNewTabValue = GetStringValueFromAttribute(xmlNode, "OpenUrlInNewTab");
            if (!string.IsNullOrWhiteSpace(openUrlInNewTabValue) && bool.TryParse(openUrlInNewTabValue, out bool booleanResult))
            {
                siteMapNode.OpenUrlInNewTab = booleanResult;
            }
        }

        private static string GetStringValueFromAttribute(XmlNode node, string attributeName)
        {
            string value = null;

            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                XmlAttribute attribute = node.Attributes[attributeName];

                if (attribute != null)
                {
                    value = attribute.Value;
                }
            }

            return value;
        }
    }
}
