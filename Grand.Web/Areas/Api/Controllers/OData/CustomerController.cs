using Grand.Api.DTOs.Customers;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        //odata/Customer(email)/AddAddress
        [HttpPost]
        public IActionResult AddAddress(string key, [FromBody] AddressDto address)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            address = _customerApiService.InsertAddress(customer, address);
            return Ok(address);
        }

        //odata/Customer(email)/UpdateAddress
        [HttpPost]
        public IActionResult UpdateAddress(string key, [FromBody] AddressDto address)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            address = _customerApiService.UpdateAddress(customer, address);
            return Ok(address);
        }
        //odata/Customer(email)/DeleteAddress
        //body: { "addressId": "xxx" }
        [HttpPost]
        public IActionResult DeleteAddress(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var addressId = parameters.FirstOrDefault(x => x.Key == "addressId").Value;
            if (addressId == null)
                return NotFound();

            var customer = _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            var address = customer.Addresses.FirstOrDefault(x => x.Id == addressId.ToString());
            if (address == null)
                return NotFound();

            _customerApiService.DeleteAddress(customer, address);
            return Ok(true);
        }


        //odata/Customer(email)/SetPassword
        //body: { "password": "123456" }
        [HttpPost]
        public IActionResult SetPassword(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var password = parameters.FirstOrDefault(x => x.Key == "password").Value;
            if (password == null)
                return NotFound();

            var changePassRequest = new ChangePasswordRequest(key, false, _customerSettings.DefaultPasswordFormat, password.ToString());
            var changePassResult = _customerRegistrationService.ChangePassword(changePassRequest);
            if (!changePassResult.Success)
            {
                return BadRequest(string.Join(',', changePassResult.Errors));
            }
            return Ok(true);

        }
    }
}
