using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class ProductAttributeController : BaseODataController
    {
        private readonly IProductAttributeApiService _productAttributeApiService;
        private readonly IPermissionService _permissionService;
        public ProductAttributeController(IProductAttributeApiService productAttributeApiService, IPermissionService permissionService)
        {
            _productAttributeApiService = productAttributeApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = _productAttributeApiService.GetById(key);
            if (productAttribute == null)
                return NotFound();

            return Ok(productAttribute);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            return Ok(_productAttributeApiService.GetProductAttributes());
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductAttributeDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _productAttributeApiService.InsertOrUpdateProductAttribute(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Attributes))
                return Forbid();

            var productAttribute = _productAttributeApiService.GetById(key);
            if (productAttribute == null)
            {
                return NotFound();
            }
            _productAttributeApiService.DeleteProductAttribute(productAttribute);
            return Ok();
        }
    }
}
