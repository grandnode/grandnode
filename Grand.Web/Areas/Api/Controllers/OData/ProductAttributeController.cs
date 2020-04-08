using Grand.Api.Commands.Models.Catalog;
using Grand.Api.DTOs.Catalog;
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
    public partial class ProductAttributeController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public ProductAttributeController(
            IMediator mediator,
            IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = await _mediator.Send(new GetQuery<ProductAttributeDto>() { Id = key });
            if (!productAttribute.Any())
                return NotFound();

            return Ok(productAttribute.FirstOrDefault());
        }

        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<ProductAttributeDto>()));
        }

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
