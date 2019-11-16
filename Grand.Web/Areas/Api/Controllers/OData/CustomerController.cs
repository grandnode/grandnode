using Grand.Api.Controllers;
using Grand.Api.DTOs.Customers;
using Grand.Api.Interfaces;
using Grand.Core.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = await _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _customerApiService.InsertOrUpdateCustomer(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _customerApiService.UpdateCustomer(model);
                return Updated(model);
            }
            return BadRequest(ModelState);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = await _customerApiService.GetByEmail(key);
            if (customer == null)
            {
                return NotFound();
            }
            await _customerApiService.DeleteCustomer(customer);
            return Ok();
        }
        //odata/Customer(email)/AddAddress
        [HttpPost]
        public async Task<IActionResult> AddAddress(string key, [FromBody] AddressDto address)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            address = await _customerApiService.InsertAddress(customer, address);
            return Ok(address);
        }

        //odata/Customer(email)/UpdateAddress
        [HttpPut]
        public async Task<IActionResult> UpdateAddress(string key, [FromBody] AddressDto address)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            address = await _customerApiService.UpdateAddress(customer, address);
            return Ok(address);
        }

        //odata/Customer(email)/DeleteAddress
        //body: { "addressId": "xxx" }
        [HttpPost]
        public async Task<IActionResult> DeleteAddress(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var addressId = parameters.FirstOrDefault(x => x.Key == "addressId").Value;
            if (addressId == null)
                return NotFound();

            var customer = await _customerApiService.GetByEmail(key);
            if (customer == null)
                return NotFound();

            var address = customer.Addresses.FirstOrDefault(x => x.Id == addressId.ToString());
            if (address == null)
                return NotFound();

            await _customerApiService.DeleteAddress(customer, address);
            return Ok(true);
        }


        //odata/Customer(email)/SetPassword
        //body: { "password": "123456" }
        [HttpPost]
        public async Task<IActionResult> SetPassword(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var password = parameters.FirstOrDefault(x => x.Key == "password").Value;
            if (password == null)
                return NotFound();

            var changePassRequest = new ChangePasswordRequest(key, false, _customerSettings.DefaultPasswordFormat, password.ToString());
            var changePassResult = await _customerRegistrationService.ChangePassword(changePassRequest);
            if (!changePassResult.Success)
            {
                return BadRequest(string.Join(',', changePassResult.Errors));
            }
            return Ok(true);

        }
    }
}
