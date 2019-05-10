using Grand.Api.Controllers;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class WarehouseController : BaseODataController
    {
        private readonly IShippingApiService _shippingApiService;
        private readonly IPermissionService _permissionService;

        public WarehouseController(IShippingApiService shippingApiService, IPermissionService permissionService)
        {
            _shippingApiService = shippingApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            var warehouse = _shippingApiService.GetWarehouses().FirstOrDefault(x => x.Id == key);
            if (warehouse == null)
                return NotFound();

            return Ok(warehouse);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            return Ok(_shippingApiService.GetWarehouses());
        }
    }
}
