using Grand.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.SalesEmployees)]
    public partial class SalesEmployeeController : BaseAdminController
    {
        #region Fields

        private readonly ISalesEmployeeService _salesEmployeeService;

        #endregion

        #region Constructors

        public SalesEmployeeController(
            ISalesEmployeeService salesEmployeeService)
        {
            _salesEmployeeService = salesEmployeeService;
        }

        #endregion

        [PermissionAuthorizeAction(PermissionActionName.List)]
        public IActionResult Index() => View();

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var weightsModel = (await _salesEmployeeService.GetAll())
                .Select(x => x.ToModel())
                .ToList();

            var gridModel = new DataSourceResult {
                Data = weightsModel,
                Total = weightsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> Update(SalesEmployeeModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var salesemployee = await _salesEmployeeService.GetSalesEmployeeById(model.Id);
            salesemployee = model.ToEntity(salesemployee);
            await _salesEmployeeService.UpdateSalesEmployee(salesemployee);

            return new NullJsonResult();
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Add(SalesEmployeeModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var salesEmployee = new SalesEmployee();
            salesEmployee = model.ToEntity(salesEmployee);
            await _salesEmployeeService.InsertSalesEmployee(salesEmployee);

            return new NullJsonResult();
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var salesemployee = await _salesEmployeeService.GetSalesEmployeeById(id);
            if (salesemployee == null)
                throw new ArgumentException("No sales employee found with the specified id");

            await _salesEmployeeService.DeleteSalesEmployee(salesemployee);

            return new NullJsonResult();
        }

    }
}
