using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Logging;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Services.Customers;
using Nop.Services.Catalog;

namespace Nop.Admin.Controllers
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

        public ActionResult ListTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var model = _customerActionService.GetCustomerActionType()
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveTypes(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            string formKey = "checkbox_action_types";
            var checkedActionTypes = form[formKey] != null ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();

            var activityTypes = _customerActionService.GetCustomerActionType();
            foreach (var actionType in activityTypes)
            {
                actionType.Enabled = checkedActionTypes.Contains(actionType.Id);
                _customerActionService.UpdateCustomerActionType(actionType);
            }
            SuccessNotification(_localizationService.GetResource("Admin.Customer.ActionType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

    
    }
}
