using Grand.Api.DTOs.Catalog;
using Grand.Api.Services;
using Grand.Framework.Security.Authorization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    [PermissionAuthorize(PermissionSystemName.Categories)]
    public partial class CategoryController : BaseODataController
    {
        private readonly ICategoryApiService _categoryApiService;
        public CategoryController(ICategoryApiService categoryApiService)
        {
            _categoryApiService = categoryApiService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            var category = _categoryApiService.GetById(key);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_categoryApiService.GetCategories());
        }

        [HttpPost]
        public IActionResult Post([FromBody] CategoryDTO model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                    model = _categoryApiService.InsertCategory(model);
                else
                    model = _categoryApiService.UpdateCategory(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            var category = _categoryApiService.GetById(key);
            if (category == null)
            {
                return NotFound();
            }
            _categoryApiService.DeleteCategory(category);
            return Ok();
        }
    }
}
