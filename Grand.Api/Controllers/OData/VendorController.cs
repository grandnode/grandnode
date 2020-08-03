using Grand.Api.DTOs.Customers;
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
    public partial class VendorController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public VendorController(IMediator mediator, IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [SwaggerOperation(summary: "Get entity from Vendor by key", OperationId = "GetVendorById")]
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Vendors))
                return Forbid();

            var vendor = await _mediator.Send(new GetQuery<VendorDto>() { Id = key });
            if (!vendor.Any())
                return NotFound();

            return Ok(vendor.FirstOrDefault());

        }

        [SwaggerOperation(summary: "Get entities from Vendor", OperationId = "GetVendors")]
        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Vendors))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<VendorDto>()));
        }
    }
}
