using Grand.Api.Controllers;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class SpecificationAttributeController : BaseODataController
    {
        private readonly ISpecificationAttributeApiService _specificationAttributeApiService;
        private readonly IPermissionService _permissionService;
        public SpecificationAttributeController(ISpecificationAttributeApiService specificationAttributeApiService, IPermissionService permissionService)
        {
            _specificationAttributeApiService = specificationAttributeApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specificationAttribute = await _specificationAttributeApiService.GetById(key);
            if (specificationAttribute == null)
                return NotFound();

            return Ok(specificationAttribute);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(_specificationAttributeApiService.GetSpecificationAttributes());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SpecificationAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _specificationAttributeApiService.InsertOrUpdateSpecificationAttribute(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] SpecificationAttributeDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _specificationAttributeApiService.UpdateSpecificationAttribute(model);
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<SpecificationAttributeDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var entity = await _specificationAttributeApiService.GetById(key);
            if (entity == null)
            {
                return NotFound();
            }

            model.Patch(entity);

            if (ModelState.IsValid)
            {
                entity = await _specificationAttributeApiService.UpdateSpecificationAttribute(entity);
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specificationAttribute = await _specificationAttributeApiService.GetById(key);
            if (specificationAttribute == null)
            {
                return NotFound();
            }
            await _specificationAttributeApiService.DeleteSpecificationAttribute(specificationAttribute);
            return Ok();
        }
    }
}
