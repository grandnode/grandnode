using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.CustomerRoles)]
    public partial class CustomerRoleController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerRoleViewModelService _customerRoleViewModelService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Constructors

        public CustomerRoleController(
            ICustomerRoleViewModelService customerRoleViewModelService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _customerRoleViewModelService = customerRoleViewModelService;
            _customerService = customerService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        #endregion

        #region Customer roles

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var customerRoles = await _customerService.GetAllCustomerRoles(command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult {
                Data = customerRoles.Select(x =>
                {
                    var rolesModel = x.ToModel();
                    return rolesModel;
                }),
                Total = customerRoles.TotalCount,
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public IActionResult Create()
        {
            var model = _customerRoleViewModelService.PrepareCustomerRoleModel();
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CustomerRoleModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerRole = await _customerRoleViewModelService.InsertCustomerRoleModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerRoles.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var customerRole = await _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = _customerRoleViewModelService.PrepareCustomerRoleModel(customerRole);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CustomerRoleModel model, bool continueEditing)
        {
            var customerRole = await _customerService.GetCustomerRoleById(model.Id);
            if (customerRole == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    if (customerRole.IsSystemRole && !model.Active)
                        throw new GrandException(_localizationService.GetResource("Admin.Customers.CustomerRoles.Fields.Active.CantEditSystem"));

                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.OrdinalIgnoreCase))
                        throw new GrandException(_localizationService.GetResource("Admin.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));

                    customerRole = await _customerRoleViewModelService.UpdateCustomerRoleModel(customerRole, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerRoles.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var customerRole = await _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
                //No customer role found with the specified id
                return RedirectToAction("List");
            if (customerRole.IsSystemRole)
                ModelState.AddModelError("", "You can't delete system role");
            try
            {
                if (ModelState.IsValid)
                {
                    await _customerRoleViewModelService.DeleteCustomerRole(customerRole);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerRoles.Deleted"));
                    return RedirectToAction("List");
                }
                ErrorNotification(ModelState);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }
        #endregion

        #region Products
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> Products(string customerRoleId, DataSourceRequest command)
        {
            var products = await _customerRoleViewModelService.PrepareCustomerRoleProductModel(customerRoleId);
            var gridModel = new DataSourceResult {
                Data = products,
                Total = products.Count()
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductDelete(string id)
        {
            var crp = await _customerService.GetCustomerRoleProductById(id);
            if (crp == null)
                throw new ArgumentException("No found the specified id");
            if (ModelState.IsValid)
            {
                await _customerService.DeleteCustomerRoleProduct(crp);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductUpdate(CustomerRoleProductModel model)
        {
            var crp = await _customerService.GetCustomerRoleProductById(model.Id);
            if (crp == null)
                throw new ArgumentException("No customer role product found with the specified id");
            if (ModelState.IsValid)
            {
                crp.DisplayOrder = model.DisplayOrder;
                await _customerService.UpdateCustomerRoleProduct(crp);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ProductAddPopup(string customerRoleId)
        {
            var model = await _customerRoleViewModelService.PrepareProductModel(customerRoleId);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, CustomerRoleProductModel.AddProductModel model)
        {
            var products = await _customerRoleViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = products.products,
                Total = products.totalCount
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(CustomerRoleProductModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _customerRoleViewModelService.InsertProductModel(model);
            }

            //a vendor should have access only to his products
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Acl

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpPost]
        public async Task<IActionResult> Acl(string customerRoleId)
        {
            var permissionRecords = await _permissionService.GetAllPermissionRecords();
            var model = new List<CustomerRolePermissionModel>();

            foreach (var pr in permissionRecords)
            {
                model.Add(new CustomerRolePermissionModel {
                    Id = pr.Id,
                    Name = pr.GetLocalizedPermissionName(_localizationService, _workContext),
                    SystemName = pr.SystemName,
                    Actions = pr.Actions.ToList(),
                    Access = pr.CustomerRoles.Contains(customerRoleId)
                });
            }

            var gridModel = new DataSourceResult {
                Data = model,
                Total = model.Count()
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> AclUpdate(string customerRoleId, string id, bool access)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageAcl))
                ModelState.AddModelError("", "You don't have permission to the update");

            var cr = await _customerService.GetCustomerRoleById(customerRoleId);
            if (cr == null)
                throw new ArgumentException("No customer role found with the specified id");

            var permissionRecord = await _permissionService.GetPermissionRecordById(id);
            if (permissionRecord == null)
                throw new ArgumentException("No permission found with the specified id");

            if (ModelState.IsValid)
            {
                if (access)
                {
                    if (!permissionRecord.CustomerRoles.Contains(customerRoleId))
                        permissionRecord.CustomerRoles.Add(customerRoleId);
                }
                else
                    if (permissionRecord.CustomerRoles.Contains(customerRoleId))
                    permissionRecord.CustomerRoles.Remove(customerRoleId);

                await _permissionService.UpdatePermissionRecord(permissionRecord);

                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }


        #endregion
    }
}
