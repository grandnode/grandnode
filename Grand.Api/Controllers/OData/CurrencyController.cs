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
    public partial class CurrencyController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public CurrencyController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Currency by key", OperationId = "GetCurrencyById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Currencies))
                return Forbid();

            var currency = await _mediator.Send(new GetQuery<CurrencyDto>() { Id = key });
            if (!currency.Any())
                return NotFound();

            return Ok(currency.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from Currency", OperationId = "GetCurrencies")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Currencies))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<CurrencyDto>()));
        }
    }
}
