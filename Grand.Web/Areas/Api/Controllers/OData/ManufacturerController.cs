using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
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

            var manufacturer = _manufacturerApiService.GetById(key);
            if (manufacturer == null)
                return NotFound();

            return Ok(manufacturer);
        }

        [HttpGet]
        [EnableQuery]
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
                model = _manufacturerApiService.InsertOrUpdateManufacturer(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Manufacturers))
                return Forbid();

            var manufacturer = _manufacturerApiService.GetById(key);
            if (manufacturer == null)
            {
                return NotFound();
            }
            _manufacturerApiService.DeleteManufacturer(manufacturer);
            return Ok();
        }
    }
}
