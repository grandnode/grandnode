using Grand.Api.Commands.Models.Customers;
using Grand.Api.DTOs.Customers;
using Grand.Api.Queries.Models.Customers;
using Grand.Domain.Customers;
using Grand.Services.Customers;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Controllers.OData
{
    public partial class CustomerController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerRegistrationService _customerRegistrationService;

        private readonly CustomerSettings _customerSettings;

        public CustomerController(
            IMediator mediator,
            IPermissionService permissionService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings)
        {
            _mediator = mediator;
            _customerRegistrationService = customerRegistrationService;
            _customerSettings = customerSettings;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Customer by key", OperationId = "GetCustomerByEmail")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = await _mediator.Send(new GetCustomerQuery() { Email = key });
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        [SwaggerOperation(summary: "Add new entity to Customer", OperationId = "InsertCustomer")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddCustomerCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in Customer", OperationId = "UpdateCustomer")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new UpdateCustomerCommand() { Model = model });
                return Updated(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity from Customer", OperationId = "DeleteCustomer")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customer = await _mediator.Send(new GetCustomerQuery() { Email = key });
            if (customer == null)
            {
                return NotFound();
            }

            await _mediator.Send(new DeleteCustomerCommand() { Email = key });

            return Ok();
        }


        //odata/Customer/AddAddress/(email)
        [SwaggerOperation(summary: "Invoke action AddAddress", OperationId = "AddAddress")]
        [Route("[action]/({key}")]
        [HttpPost]
        public async Task<IActionResult> AddAddress(string key, [FromBody] AddressDto address)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _mediator.Send(new GetCustomerQuery() { Email = key });
            if (customer == null)
                return NotFound();

            address = await _mediator.Send(new AddCustomerAddressCommand() { Customer = customer, Address = address });
            return Ok(address);
        }

        //odata/Customer/UpdateAddress/(email)
        [SwaggerOperation(summary: "Invoke action UpdateAddress", OperationId = "UpdateAddress")]
        [Route("[action]/({key})")]
        [HttpPost]
        public async Task<IActionResult> UpdateAddress(string key, [FromBody] AddressDto address)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _mediator.Send(new GetCustomerQuery() { Email = key });
            if (customer == null)
                return NotFound();

            address = await _mediator.Send(new UpdateCustomerAddressCommand() { Customer = customer, Address = address });

            return Ok(address);
        }

        //odata/Customer/DeleteAddress/(email)
        //body: { "addressId": "xxx" }
        [SwaggerOperation(summary: "Invoke action DeleteAddress", OperationId = "DeleteAddress")]
        [Route("[action]/({key})")]
        [HttpPost]
        public async Task<IActionResult> DeleteAddress(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var addressId = parameters.FirstOrDefault(x => x.Key == "addressId").Value;
            if (addressId == null)
                return NotFound();

            var customer = await _mediator.Send(new GetCustomerQuery() { Email = key });
            if (customer == null)
                return NotFound();

            var address = customer.Addresses.FirstOrDefault(x => x.Id == addressId.ToString());
            if (address == null)
                return NotFound();

            await _mediator.Send(new DeleteCustomerAddressCommand() { Customer = customer, Address = address });

            return Ok(true);
        }


        //odata/Customer/SetPassword/(email)
        //body: { "password": "123456" }
        [SwaggerOperation(summary: "Invoke action SetPassword", OperationId = "SetPassword")]
        [Route("[action]/({key})")]
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
