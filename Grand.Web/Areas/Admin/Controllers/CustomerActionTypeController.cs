using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Actions)]
    public partial class CustomerActionTypeController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerActionService _customerActionService;
        private readonly ILocalizationService _localizationService;
        #endregion Fields

        #region Constructors

        public CustomerActionTypeController(
            ILocalizationService localizationService,
            ICustomerActionService customerActionService
            )
        {
            _customerActionService = customerActionService;
            _localizationService = localizationService;
        }

        #endregion

        #region Action types

        public async Task<IActionResult> ListTypes()
        {
            var model = (await _customerActionService.GetCustomerActionType())
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTypes(IFormCollection form)
        {
            string formKey = "checkbox_action_types";
            var checkedActionTypes = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            var activityTypes = await _customerActionService.GetCustomerActionType();
            foreach (var actionType in activityTypes)
            {
                actionType.Enabled = checkedActionTypes.Contains(actionType.Id);
                await _customerActionService.UpdateCustomerActionType(actionType);
            }
            SuccessNotification(_localizationService.GetResource("Admin.Customers.ActionType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

    
    }
}
