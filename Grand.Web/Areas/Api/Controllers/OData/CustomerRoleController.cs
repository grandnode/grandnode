using Grand.Api.DTOs.Customers;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class CustomerRoleController : BaseODataController
    {
        private readonly ICustomerRoleApiService _customerRoleApiService;
        private readonly IPermissionService _permissionService;
        public CustomerRoleController(ICustomerRoleApiService customerRoleApiService, IPermissionService permissionService)
        {
            _customerRoleApiService = customerRoleApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = _customerRoleApiService.GetById(key);
            if (customerRole == null)
                return NotFound();

            return Ok(customerRole);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            return Ok(_customerRoleApiService.GetCustomerRoles());
        }

        [HttpPost]
        public IActionResult Post([FromBody] CustomerRoleDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _customerRoleApiService.InsertOrUpdateCustomerRole(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = _customerRoleApiService.GetById(key);
            if (customerRole == null)
            {
                return NotFound();
            }
            if (customerRole.IsSystemRole)
            {
                return Forbid();
            }

            _customerRoleApiService.DeleteCustomerRole(customerRole);
            return Ok();
        }
    }
}
