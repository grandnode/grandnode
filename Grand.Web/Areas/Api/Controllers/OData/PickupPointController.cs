using Grand.Api.Controllers;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class PickupPointController : BaseODataController
    {
        private readonly IShippingApiService _shippingApiService;
        private readonly IPermissionService _permissionService;

        public PickupPointController(IShippingApiService shippingApiService, IPermissionService permissionService)
        {
            _shippingApiService = shippingApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            var points = _shippingApiService.GetPickupPoints().FirstOrDefault(x => x.Id == key);
            if (points == null)
                return NotFound();

            return Ok(points);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            return Ok(_shippingApiService.GetPickupPoints());
        }
    }
}
