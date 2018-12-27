using Grand.Api.DTOs.Customers;
using Grand.Api.Services;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class CustomerController : BaseODataController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        private readonly CustomerSettings _customerSettings;

        public CustomerController(ICustomerApiService customerApiService, IPermissionService permissionService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings)
        {
            _customerApiService = customerApiService;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
       
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _customerApiService.InsertOrUpdateCustomer(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = _customerApiService.GetByEmail(key);
            if (customer == null)
            {
                return NotFound();
            }
            _customerApiService.DeleteCustomer(customer);
            return Ok();
        }

        //api/Customer(email)/ChangePassword - body contains text with password
        [HttpPost]
        public IActionResult ChangePassword(string key)
        {
            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                string password = stream.ReadToEnd();
                if(!string.IsNullOrEmpty(password))
                {
                    var changePassRequest = new ChangePasswordRequest(key, false, _customerSettings.DefaultPasswordFormat, password);
                    var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        return BadRequest(string.Join(',', changePassResult.Errors));
                    }
                    return Ok(true);
                }
                return Ok(false);
            }
        }
    }
}
