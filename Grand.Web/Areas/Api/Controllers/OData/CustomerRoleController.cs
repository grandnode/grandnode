using Grand.Api.Commands.Models.Customers;
using Grand.Api.DTOs.Customers;
using Grand.Api.Queries.Models.Common;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
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

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = key });
            if (!customerRole.Any())
                return NotFound();

            return Ok(customerRole.FirstOrDefault());
        }

        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<CustomerRoleDto>()));
        }

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

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<CustomerRoleDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Customers))
                return Forbid();

            var customerRole = await _mediator.Send(new GetQuery<CustomerRoleDto>() { Id = key });
            if (!customerRole.Any())
            {
                return NotFound();
            }
            var cr = customerRole.FirstOrDefault();
            model.Patch(cr);

            if (ModelState.IsValid && !cr.IsSystemRole)
            {
                await _mediator.Send(new UpdateCustomerRoleCommand() { Model = cr });
                return Ok();
            }

            return BadRequest(ModelState);
        }

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
