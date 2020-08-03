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
    
    public partial class CategoryController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        public CategoryController(
            IMediator mediator,
            IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Category by key", OperationId = "GetCategoryById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _mediator.Send(new GetQuery<CategoryDto>() { Id = key });
            if (!category.Any())
                return NotFound();

            return Ok(category.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from Category", OperationId = "GetCategories")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<CategoryDto>()));
        }

        [SwaggerOperation(summary: "Add new entity to Category", OperationId = "InsertCategory")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CategoryDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddCategoryCommand() { Model = model });
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [SwaggerOperation(summary: "Update entity in Category", OperationId = "UpdateCategory")]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CategoryDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _mediator.Send(new GetQuery<CategoryDto>() { Id = model.Id });
            if (!category.Any())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new UpdateCategoryCommand() { Model = model });
                return Ok(model);
            }
            return BadRequest(ModelState);
        }
        [SwaggerOperation(summary: "Update entity in Category (delta)", OperationId = "UpdateCategoryPatch")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, JsonPatchDocument<CategoryDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _mediator.Send(new GetQuery<CategoryDto>() { Id = key });
            if (!category.Any())
            {
                return NotFound();
            }
            var cat = category.FirstOrDefault();
            model.ApplyTo(cat);

            if (ModelState.IsValid)
            {
                await _mediator.Send(new UpdateCategoryCommand() { Model = cat });
                return Ok();
            }
            return BadRequest(ModelState);
        }
        [SwaggerOperation(summary: "Delete entity from Category", OperationId = "DeleteCategory")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _mediator.Send(new GetQuery<CategoryDto>() { Id = key });
            if (!category.Any())
            {
                return NotFound();
            }

            await _mediator.Send(new DeleteCategoryCommand() { Model = category.FirstOrDefault() });

            return Ok();
        }
    }
}
