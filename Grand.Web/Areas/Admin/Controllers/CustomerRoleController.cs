using Grand.Api.Extensions;
using Grand.Core;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
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
        #endregion

        #region Constructors

        public CustomerRoleController(
            ICustomerRoleViewModelService customerRoleViewModelService,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            this._customerRoleViewModelService = customerRoleViewModelService;
            this._customerService = customerService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Customer roles

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

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

        public IActionResult Create()
        {
            var model = _customerRoleViewModelService.PrepareCustomerRoleModel();
            return View(model);
        }

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

        public async Task<IActionResult> Edit(string id)
        {
            var customerRole = await _customerService.GetCustomerRoleById(id);
            if (customerRole == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = _customerRoleViewModelService.PrepareCustomerRoleModel(customerRole);
            return View(model);
        }

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

        public async Task<IActionResult> ProductAddPopup(string customerRoleId)
        {
            var model = await _customerRoleViewModelService.PrepareProductModel(customerRoleId);
            return View(model);
        }

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
    }
}
