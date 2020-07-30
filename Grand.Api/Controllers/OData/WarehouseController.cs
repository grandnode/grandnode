using Grand.Api.DTOs.Shipping;
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
    public partial class WarehouseController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public WarehouseController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Warehouse by key", OperationId = "GetWarehouseById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            var warehouse = await _mediator.Send(new GetQuery<WarehouseDto>() { Id = key });
            if (!warehouse.Any())
                return NotFound();

            return Ok(warehouse.FirstOrDefault());
        }

        [SwaggerOperation(summary: "Get entities from Warehouse", OperationId = "GetWarehouses")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.ShippingSettings))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<WarehouseDto>()));
        }
    }
}
