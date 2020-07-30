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
    public partial class ManufacturerTemplateController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public ManufacturerTemplateController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from ManufacturerTemplate by key", OperationId = "GetManufacturerTemplateById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Maintenance))
                return Forbid();

            var template = await _mediator.Send(new GetMessageTemplateQuery() { Id = key, TemplateName = typeof(Domain.Catalog.ManufacturerTemplate).Name });
            if (!template.Any())
                return NotFound();

            return Ok(template.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from ManufacturerTemplate", OperationId = "GetManufacturerTemplates")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Maintenance))
                return Forbid();

            return Ok(await _mediator.Send(new GetMessageTemplateQuery() { TemplateName = typeof(Domain.Catalog.ManufacturerTemplate).Name }));
        }
    }
}
