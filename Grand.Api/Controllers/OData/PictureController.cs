using Grand.Api.Commands.Models.Common;
using Grand.Api.DTOs.Common;
using Grand.Api.Queries.Models.Common;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace Grand.Api.Controllers.OData
{
    public partial class PictureController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public PictureController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entities from Picture by key", OperationId = "GetPictureById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = await _mediator.Send(new GetPictureByIdQuery() { Id = key });
            if (picture == null)
                return NotFound();

            return Ok(picture);
        }

        [SwaggerOperation(summary: "Add new entity in Picture", OperationId = "InsertPicture")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PictureDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddPictureCommand() { PictureDto = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Delete entity in Picture", OperationId = "DeletePicture")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Files))
                return Forbid();

            var picture = await _mediator.Send(new GetPictureByIdQuery() { Id = key });
            if (picture == null)
            {
                return NotFound();
            }
            await _mediator.Send(new DeletePictureCommand() { PictureDto = picture });
            return Ok();
        }
    }
}
