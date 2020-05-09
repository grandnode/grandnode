using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Models;
using Grand.Services.Commands.Models.Security;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
	{
		#region Fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IMediator _mediator;

        #endregion

        #region Constructors

        public SecurityController(
            ILogger logger, 
            IWorkContext workContext,
            IPermissionService permissionService,
            ICustomerService customerService, 
            ILocalizationService localizationService,
            IMediator mediator)
		{
            _logger = logger;
            _workContext = workContext;
            _permissionService = permissionService;
            _customerService = customerService;
            _localizationService = localizationService;
            _mediator = mediator;
        }

		#endregion 

        #region Methods

        public IActionResult AccessDenied(string pageUrl)
        {
            var currentCustomer = _workContext.CurrentCustomer;
            if (currentCustomer == null || currentCustomer.IsGuest())
            {
                _logger.Information(string.Format("Access denied to anonymous request on {0}", pageUrl));
                return View();
            }

            _logger.Information(string.Format("Access denied to user #{0} '{1}' on {2}", currentCustomer.Email, currentCustomer.Email, pageUrl));


            return View();
        }

        public async Task<IActionResult> Permissions()
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var model = new PermissionMappingModel();

            var permissionRecords = await _permissionService.GetAllPermissionRecords();
            var customerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);
            foreach (var pr in permissionRecords)
            {
                model.AvailablePermissions.Add(new PermissionRecordModel
                {
                    //Name = pr.Name,
                    Name = pr.GetLocalizedPermissionName(_localizationService, _workContext),
                    SystemName = pr.SystemName
                });
            }
            foreach (var cr in customerRoles)
            {
                model.AvailableCustomerRoles.Add(new CustomerRoleModel() { Id = cr.Id, Name = cr.Name });
            }
            foreach (var pr in permissionRecords)
                foreach (var cr in customerRoles)
                {
                    bool allowed = pr.CustomerRoles.Count(x => x == cr.Id) > 0;
                    if (!model.Allowed.ContainsKey(pr.SystemName))
                        model.Allowed[pr.SystemName] = new Dictionary<string, bool>();
                    model.Allowed[pr.SystemName][cr.Id] = allowed;
                }

            return View(model);
        }

        [HttpPost, ActionName("Permissions"), ParameterBasedOnFormName("save-continue", "install")]
        public async Task<IActionResult> PermissionsSave(IFormCollection form, bool install)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            if (!install)
            {
                var permissionRecords = await _permissionService.GetAllPermissionRecords();
                var customerRoles = await _customerService.GetAllCustomerRoles(showHidden: true);

                foreach (var cr in customerRoles)
                {
                    string formKey = "allow_" + cr.Id;
                    var permissionRecordSystemNamesToRestrict = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                    foreach (var pr in permissionRecords)
                    {

                        bool allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);
                        if (allow)
                        {

                            if (pr.CustomerRoles.FirstOrDefault(x => x == cr.Id) == null)
                            {
                                pr.CustomerRoles.Add(cr.Id);
                                await _permissionService.UpdatePermissionRecord(pr);
                            }
                        }
                        else
                        {
                            if (pr.CustomerRoles.FirstOrDefault(x => x == cr.Id) != null)
                            {
                                pr.CustomerRoles.Remove(cr.Id);
                                await _permissionService.UpdatePermissionRecord(pr);
                            }
                        }
                    }
                }
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.ACL.Updated"));
            }
            else
            {
                IPermissionProvider provider = new StandardPermissionProvider();
                await _mediator.Send(new InstallNewPermissionsCommand() { PermissionProvider = provider });

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.ACL.Installed"));
            }
            return RedirectToAction("Permissions");
        }

        #endregion
    }
}
