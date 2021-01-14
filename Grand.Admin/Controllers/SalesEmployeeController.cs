using Grand.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Customers;
using Grand.Services.Documents;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Admin.Extensions;
using Grand.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.SalesEmployees)]
    public partial class SalesEmployeeController : BaseAdminController
    {
        #region Fields

        private readonly ISalesEmployeeService _salesEmployeeService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IDocumentService _documentService;

        #endregion

        #region Constructors

        public SalesEmployeeController(
            ISalesEmployeeService salesEmployeeService,
            ICustomerService customerService,
            IOrderService orderService,
            IDocumentService documentService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _salesEmployeeService = salesEmployeeService;
            _customerService = customerService;
            _orderService = orderService;
            _documentService = documentService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
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

            //activity log
            await _customerActivityService.InsertActivity("EditSalesEmployee", salesemployee.Id, _localizationService.GetResource("ActivityLog.EditSalesEmployee"),
                salesemployee.Name);

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

            //activity log
            await _customerActivityService.InsertActivity("AddNewSalesEmployee", salesEmployee.Id, _localizationService.GetResource("ActivityLog.AddNewSalesEmployee"),
                salesEmployee.Name);

            return new NullJsonResult();
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var salesemployee = await _salesEmployeeService.GetSalesEmployeeById(id);
            if (salesemployee == null)
                throw new ArgumentException("No sales employee found with the specified id");

            var customers = await _customerService.GetAllCustomers(salesEmployeeId: id);
            if (customers.Any())
                return Json(new DataSourceResult { Errors = "Sales employee is related with customers" });

            var orders = await _orderService.SearchOrders(salesEmployeeId: id);
            if (orders.Any())
                return Json(new DataSourceResult { Errors = "Sales employee is related with orders" });

            var documents = await _documentService.GetAll(seId: id);
            if (documents.Any())
                return Json(new DataSourceResult { Errors = "Sales employee is related with documents" });

            await _salesEmployeeService.DeleteSalesEmployee(salesemployee);

            //activity log
            await _customerActivityService.InsertActivity("DeleteSalesEmployee", salesemployee.Id, _localizationService.GetResource("ActivityLog.DeleteSalesEmployee"),
                salesemployee.Name);

            return new NullJsonResult();
        }

    }
}
