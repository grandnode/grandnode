using Grand.Api.DTOs.Common;
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
    public partial class StateProvinceController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public StateProvinceController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from StateProvince by key", OperationId = "GetStateProvinceById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Countries))
                return Forbid();

            var states = await _mediator.Send(new GetQuery<StateProvinceDto>() { Id = key });
            if (!states.Any())
                return NotFound();

            return Ok(states);
        }

        [SwaggerOperation(summary: "Get entities from StateProvince", OperationId = "GetStateProvinces")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Countries))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<StateProvinceDto>()));
        }
    }
}
