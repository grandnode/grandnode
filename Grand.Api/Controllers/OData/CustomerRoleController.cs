using Grand.Api.Commands.Models.Customers;
using Grand.Api.DTOs.Customers;
using Grand.Api.Queries.Models.Common;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Controllers.OData
{
    public partial class CustomerRoleController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public CustomerRoleController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from CustomerRole by key", OperationId = "GetCustomerRoleById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = key });
            if (!customerRole.Any())
                return NotFound();

            return Ok(customerRole.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from CustomerRole", OperationId = "GetCustomerRoles")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<CustomerRoleDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to CustomerRole", OperationId = "InsertCustomerRole")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerRoleDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddCustomerRoleCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in CustomerRole", OperationId = "UpdateCustomerRole")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CustomerRoleDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = model.Id });
            if (!customerRole.Any())
            {
                return NotFound();
            }

            if (ModelState.IsValid && !model.IsSystemRole)
            {
                model = await _mediator.Send(new UpdateCustomerRoleCommand() { Model = model });
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Partially update entity in CustomerRole", OperationId = "PartiallyUpdateCustomerRole")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, JsonPatchDocument<CustomerRoleDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = key });
            if (!customerRole.Any())
            {
                return NotFound();
            }
            var cr = customerRole.FirstOrDefault();
            model.ApplyTo(cr);

            if (ModelState.IsValid && !cr.IsSystemRole)
            {
                await _mediator.Send(new UpdateCustomerRoleCommand() { Model = cr });
                return Ok();
            }

            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity in CustomerRole", OperationId = "DeleteCustomerRole")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = key });
            if (!customerRole.Any())
            {
                return NotFound();
            }

            if (customerRole.FirstOrDefault().IsSystemRole)
            {
                return Forbid();
            }
            await _mediator.Send(new DeleteCustomerRoleCommand() { Model = customerRole.FirstOrDefault() });

            return Ok();
        }
    }
}
