using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specificationAttribute = _specificationAttributeApiService.GetById(key);
            if (specificationAttribute == null)
                return NotFound();

            return Ok(specificationAttribute);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(_specificationAttributeApiService.GetSpecificationAttributes());
        }

        [HttpPost]
        public IActionResult Post([FromBody] SpecificationAttributeDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _specificationAttributeApiService.InsertOrUpdateSpecificationAttribute(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var specificationAttribute = _specificationAttributeApiService.GetById(key);
            if (specificationAttribute == null)
            {
                return NotFound();
            }
            _specificationAttributeApiService.DeleteSpecificationAttribute(specificationAttribute);
            return Ok();
        }
    }
}
