using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Logging;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Framework.Kendoui;
using Grand.Web.Framework.Mvc;
using Grand.Services.Customers;
using Grand.Services.Catalog;

namespace Grand.Admin.Controllers
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
            var checkedActionTypes = form[formKey] != null ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

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
