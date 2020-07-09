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
    public partial class ProductAttributeODataController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public ProductAttributeODataController(
            IMediator mediator,
            IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from ProductAttribute")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = await _mediator.Send(new GetQuery<ProductAttributeDto>() { Id = key });
            if (!productAttribute.Any())
                return NotFound();

            return Ok(productAttribute.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from ProductAttribute")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<ProductAttributeDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to ProductAttribute")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddProductAttributeCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in ProductAttribute")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProductAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new UpdateProductAttributeCommand() { Model = model });
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Partially update entity in ProductAttribute")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<ProductAttributeDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = await _mediator.Send(new GetQuery<ProductAttributeDto>() { Id = key });
            if (!productAttribute.Any())
                return NotFound();

            var pa = productAttribute.FirstOrDefault();
            model.Patch(pa);

            if (ModelState.IsValid)
            {
                pa = await _mediator.Send(new UpdateProductAttributeCommand() { Model = pa });
                return Ok(pa);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity from ProductAttribute")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = await _mediator.Send(new GetQuery<ProductAttributeDto>() { Id = key });
            if (!productAttribute.Any())
                return NotFound();

            await _mediator.Send(new DeleteProductAttributeCommand() { Model = productAttribute.FirstOrDefault() });
            return Ok();
        }
    }
}
