using Grand.Api.Controllers;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class ManufacturerController : BaseODataController
    {
        private readonly IManufacturerApiService _manufacturerApiService;
        private readonly IPermissionService _permissionService;
        public ManufacturerController(IManufacturerApiService manufacturerApiService, IPermissionService permissionService)
        {
            _manufacturerApiService = manufacturerApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _manufacturerApiService.GetById(key);
            if (manufacturer == null)
                return NotFound();

            return Ok(manufacturer);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            return Ok(_manufacturerApiService.GetManufacturers());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ManufacturerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _manufacturerApiService.InsertOrUpdateManufacturer(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ManufacturerDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _manufacturerApiService.UpdateManufacturer(model);
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<ManufacturerDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var entity = await _manufacturerApiService.GetById(key);
            if (entity == null)
            {
                return NotFound();
            }

            model.Patch(entity);

            if (ModelState.IsValid)
            {
                entity = await _manufacturerApiService.UpdateManufacturer(entity);
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = await _manufacturerApiService.GetById(key);
            if (manufacturer == null)
            {
                return NotFound();
            }
            await _manufacturerApiService.DeleteManufacturer(manufacturer);
            return Ok();
        }
    }
}
