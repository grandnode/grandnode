using Grand.Api.Commands.Models.Catalog;
using Grand.Api.DTOs.Catalog;
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
    public partial class ManufacturerController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        public ManufacturerController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Manufacturer by key", OperationId = "GetManufacturerById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _mediator.Send(new GetQuery<ManufacturerDto>() { Id = key });
            if (!manufacturer.Any())
                return NotFound();

            return Ok(manufacturer);
        }

        [SwaggerOperation(summary: "Get entities from Manufacturer", OperationId = "GetManufacturers")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<ManufacturerDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to Manufacturer", OperationId = "InsertManufacturer")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ManufacturerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddManufacturerCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in Manufacturer", OperationId = "UpdateManufacturer")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ManufacturerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();


            var manufacturer = await _mediator.Send(new GetQuery<ManufacturerDto>() { Id = model.Id });
            if (!manufacturer.Any())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new UpdateManufacturerCommand() { Model = model });
                return Ok(model);
            }

            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Partially update entity in Manufacturer", OperationId = "PartiallyUpdateManufacturer")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, JsonPatchDocument<ManufacturerDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _mediator.Send(new GetQuery<ManufacturerDto>() { Id = key });
            if (!manufacturer.Any())
            {
                return NotFound();
            }
            var man = manufacturer.FirstOrDefault();
            model.ApplyTo(man);

            if (ModelState.IsValid)
            {
                await _mediator.Send(new UpdateManufacturerCommand() { Model = man });
                return Ok();
            }

            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity in Manufacturer", OperationId = "DeleteManufacturer")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _mediator.Send(new GetQuery<ManufacturerDto>() { Id = key });
            if (!manufacturer.Any())
            {
                return NotFound();
            }

            await _mediator.Send(new DeleteManufacturerCommand() { Model = manufacturer.FirstOrDefault() });

            return Ok();
        }
    }
}
