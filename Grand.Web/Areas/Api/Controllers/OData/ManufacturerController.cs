using Grand.Api.DTOs.Catalog;
using Grand.Api.Services;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var Manufacturer = _manufacturerApiService.GetById(key);
            if (Manufacturer == null)
                return NotFound();

            return Ok(Manufacturer);
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            return Ok(_manufacturerApiService.GetManufacturers());
        }

        [HttpPost]
        public IActionResult Post([FromBody] ManufacturerDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                    model = _manufacturerApiService.InsertManufacturer(model);
                else
                    model = _manufacturerApiService.UpdateManufacturer(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var Manufacturer = _manufacturerApiService.GetById(key);
            if (Manufacturer == null)
            {
                return NotFound();
            }
            _manufacturerApiService.DeleteManufacturer(Manufacturer);
            return Ok();
        }
    }
}
