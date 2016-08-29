using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Models.Customers;
using Grand.Admin.Models.Security;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;

namespace Grand.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
	{
		#region Fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

        public SecurityController(ILogger logger, IWorkContext workContext,
            IPermissionService permissionService,
            ICustomerService customerService, ILocalizationService localizationService)
		{
            this._logger = logger;
            this._workContext = workContext;
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public ActionResult AccessDenied(string pageUrl)
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

        public ActionResult Permissions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var model = new PermissionMappingModel();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);
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
                model.AvailableCustomerRoles.Add(new CustomerRoleModel
                {
                    Id = cr.Id,
                    Name = cr.Name
                });
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

        [HttpPost, ActionName("Permissions")]
        public ActionResult PermissionsSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var permissionRecords = _permissionService.GetAllPermissionRecords();
            var customerRoles = _customerService.GetAllCustomerRoles(true);


            foreach (var cr in customerRoles)
            {
                string formKey = "allow_" + cr.Id;
                var permissionRecordSystemNamesToRestrict = form[formKey] != null ? form[formKey].Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                //cr.PermissionRecords.Clear();
                foreach (var pr in permissionRecords)
                {

                    bool allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);
                    if (allow)
                    {
                        
                        if (pr.CustomerRoles.FirstOrDefault(x => x == cr.Id) == null)
                        {
                            pr.CustomerRoles.Add(cr.Id);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                    else
                    {
                        if (pr.CustomerRoles.FirstOrDefault(x => x == cr.Id) != null)
                        {
                            pr.CustomerRoles.Remove(cr.Id);
                            _permissionService.UpdatePermissionRecord(pr);
                        }
                    }
                }
            }

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ACL.Updated"));
            return RedirectToAction("Permissions");
        }

        #endregion
    }
}
