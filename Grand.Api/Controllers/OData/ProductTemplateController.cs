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
    public partial class ProductTemplateController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public ProductTemplateController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from ProductTemplate by key", OperationId = "GetProductTemplateById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Maintenance))
                return Forbid();

            var template = await _mediator.Send(new GetMessageTemplateQuery() { Id = key, TemplateName = typeof(Domain.Catalog.ProductTemplate).Name });
            if (!template.Any())
                return NotFound();

            return Ok(template.FirstOrDefault());

        }

        [SwaggerOperation(summary: "Get entities from ProductTemplate", OperationId = "GetProductTemplates")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Maintenance))
                return Forbid();

            return Ok(await _mediator.Send(new GetMessageTemplateQuery() { TemplateName = typeof(Domain.Catalog.ProductTemplate).Name }));
        }
    }
}
