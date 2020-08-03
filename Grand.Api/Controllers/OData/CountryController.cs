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
    public partial class CountryController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public CountryController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Country by key", OperationId = "GetCountryById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Countries))
                return Forbid();

            var country = await _mediator.Send(new GetQuery<CountryDto>() { Id = key });
            if (!country.Any())
                return NotFound();

            return Ok(country.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from Country", OperationId = "GetCountries")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Countries))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<CountryDto>()));
        }
    }
}
