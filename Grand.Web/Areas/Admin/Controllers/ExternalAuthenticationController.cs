using Grand.Core.Domain.Customers;
using Grand.Core.Plugins;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Services.Authentication.External;
using Grand.Services.Configuration;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.ExternalAuthentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ExternalAuthenticationController : BaseAdminController
	{
		#region Fields

        private readonly IExternalAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IPluginFinder _pluginFinder;

		#endregion

		#region Constructors

        public ExternalAuthenticationController(IExternalAuthenticationService openAuthenticationService, 
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ISettingService settingService, IPermissionService permissionService,
            IPluginFinder pluginFinder)
		{
            this._openAuthenticationService = openAuthenticationService;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._pluginFinder = pluginFinder;
		}

		#endregion 

        #region Methods

        public IActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult Methods(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var methodsModel = new List<AuthenticationMethodModel>();
            var methods = _openAuthenticationService.LoadAllExternalAuthenticationMethods();
            foreach (var method in methods)
            {
                var tmp1 = method.ToModel();
                tmp1.IsActive = method.IsMethodActive(_externalAuthenticationSettings);
                tmp1.ConfigurationUrl = method.GetConfigurationPageUrl();
                methodsModel.Add(tmp1);
            }
            methodsModel = methodsModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = methodsModel,
                Total = methodsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult MethodUpdate( AuthenticationMethodModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var eam = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(model.SystemName);
            if (eam.IsMethodActive(_externalAuthenticationSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(eam.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_externalAuthenticationSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(eam.PluginDescriptor.SystemName);
                    _settingService.SaveSetting(_externalAuthenticationSettings);
                }
            }
            var pluginDescriptor = eam.PluginDescriptor;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        #endregion
    }
}