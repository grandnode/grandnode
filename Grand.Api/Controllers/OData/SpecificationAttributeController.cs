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
    public partial class SpecificationAttributeController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        public SpecificationAttributeController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from SpecificationAttribute by key", OperationId = "GetSpecificationAttributeById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specificationAttribute = await _mediator.Send(new GetQuery<SpecificationAttributeDto>() { Id = key });
            if (!specificationAttribute.Any())
                return NotFound();

            return Ok(specificationAttribute.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from SpecificationAttribute", OperationId = "GetSpecificationAttributes")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<SpecificationAttributeDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to SpecificationAttribute", OperationId = "InsertSpecificationAttribute")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SpecificationAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddSpecificationAttributeCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in SpecificationAttribute", OperationId = "UpdateSpecificationAttribute")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] SpecificationAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new UpdateSpecificationAttributeCommand() { Model = model });
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Partially update entity in SpecificationAttribute", OperationId = "PartiallyUpdateSpecificationAttribute")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, JsonPatchDocument<SpecificationAttributeDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();


            var specification = await _mediator.Send(new GetQuery<SpecificationAttributeDto>() { Id = key });
            if (!specification.Any())
            {
                return NotFound();
            }
            var spec = specification.FirstOrDefault();
            model.ApplyTo(spec);

            if (ModelState.IsValid)
            {
                await _mediator.Send(new UpdateSpecificationAttributeCommand() { Model = spec });
                return Ok();
            }
            return BadRequest(ModelState);

        }

        [SwaggerOperation(summary: "Delete entity in SpecificationAttribute", OperationId = "DeleteSpecificationAttribute")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specification = await _mediator.Send(new GetQuery<SpecificationAttributeDto>() { Id = key });
            if (!specification.Any())
            {
                return NotFound();
            }
            await _mediator.Send(new DeleteSpecificationAttributeCommand() { Model = specification.FirstOrDefault() });

            return Ok();
        }
    }
}
