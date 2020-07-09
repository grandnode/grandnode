using Grand.Api.Commands.Models.Catalog;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Queries.Models.Common;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Controllers.OData
{
    public partial class ManufacturerODataController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        public ManufacturerODataController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Manufacturers")]
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

        [SwaggerOperation(summary: "Get entities from Manufacturer")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<ManufacturerDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to Manufacturer")]
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

        [SwaggerOperation(summary: "Update entity in Manufacturer")]
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

        [SwaggerOperation(summary: "Partially update entity in Manufacturer")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<ManufacturerDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _mediator.Send(new GetQuery<ManufacturerDto>() { Id = key });
            if (!manufacturer.Any())
            {
                return NotFound();
            }
            var man = manufacturer.FirstOrDefault();
            model.Patch(man);

            if (ModelState.IsValid)
            {
                await _mediator.Send(new UpdateManufacturerCommand() { Model = man });
                return Ok();
            }

            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity in Manufacturer")]
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
