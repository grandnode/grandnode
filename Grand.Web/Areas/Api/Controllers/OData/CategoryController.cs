using Grand.Api.Controllers;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class CategoryController : BaseODataController
    {
        private readonly ICategoryApiService _categoryApiService;
        private readonly IPermissionService _permissionService;
        public CategoryController(ICategoryApiService categoryApiService, IPermissionService permissionService)
        {
            _categoryApiService = categoryApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _categoryApiService.GetById(key);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            return Ok(_categoryApiService.GetCategories());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CategoryDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _categoryApiService.InsertOrUpdateCategory(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CategoryDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _categoryApiService.UpdateCategory(model);
                return Ok(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<CategoryDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var entity = await _categoryApiService.GetById(key);
            if (entity == null)
            {
                return NotFound();
            }

            model.Patch(entity);

            if (ModelState.IsValid)
            {
                entity = await _categoryApiService.UpdateCategory(entity);
                return Ok(entity);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = await _categoryApiService.GetById(key);
            if (category == null)
            {
                return NotFound();
            }
            await _categoryApiService.DeleteCategory(category);
            return Ok();
        }
    }
}
