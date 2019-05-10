using Grand.Api.Controllers;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class VendorController : BaseODataController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IPermissionService _permissionService;

        public VendorController(ICustomerApiService customerApiService, IPermissionService permissionService)
        {
            _customerApiService = customerApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Vendors))
                return Forbid();

            var vendor = await _customerApiService.GetVendorById(key);
            if (vendor == null)
                return NotFound();

            return Ok(vendor);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Vendors))
                return Forbid();

            return Ok(_customerApiService.GetVendors());
        }
    }
}
