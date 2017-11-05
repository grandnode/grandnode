using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerActionTypeController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICustomerActionService _customerActionService;
        private readonly ILocalizationService _localizationService;
        #endregion Fields

        #region Constructors

        public CustomerActionTypeController(            
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ICustomerActionService customerActionService
            )
        {
            this._customerActionService = customerActionService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Action types

        public IActionResult ListTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var model = _customerActionService.GetCustomerActionType()
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveTypes(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            string formKey = "checkbox_action_types";
            var checkedActionTypes = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            var activityTypes = _customerActionService.GetCustomerActionType();
            foreach (var actionType in activityTypes)
            {
                actionType.Enabled = checkedActionTypes.Contains(actionType.Id);
                _customerActionService.UpdateCustomerActionType(actionType);
            }
            SuccessNotification(_localizationService.GetResource("Admin.Customers.ActionType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

    
    }
}
