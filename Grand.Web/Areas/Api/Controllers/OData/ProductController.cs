using Grand.Api.DTOs.Catalog;
using Grand.Api.Services;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class ProductController : BaseODataController
    {
        private readonly IProductApiService _productApiService;
        private readonly IPermissionService _permissionService;
        public ProductController(IProductApiService productApiService, IPermissionService permissionService)
        {
            _productApiService = productApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            return Ok(_productApiService.GetProducts());
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = _productApiService.InsertOrUpdateProduct(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }
            _productApiService.DeleteProduct(product);

            return Ok();
        }

        //odata/Product(id)/UpdateStock
        //body: { "Stock": 10 }
        [HttpPost]
        public IActionResult UpdateStock(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            if (parameters == null)
                return NotFound();

            var warehouseId = parameters.FirstOrDefault(x => x.Key == "WarehouseId").Value;
            var stock = parameters.FirstOrDefault(x => x.Key == "Stock").Value;
            if (stock != null)
            {
                if (int.TryParse(stock.ToString(), out int stockqty))
                {
                    _productApiService.UpdateStock(product, warehouseId?.ToString(), stockqty);
                    return Ok(true);
                }
            }
            return Ok(false);
        }

        [HttpPost]
        public IActionResult CreateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pc = product.Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc != null)
                ModelState.AddModelError("", "Product category mapping found with the specified categoryid");

            if (ModelState.IsValid)
            {
                if (productCategory == null)
                    return NotFound();

                _productApiService.InsertProductCategory(product, productCategory);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pc = product.Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc == null)
                ModelState.AddModelError("", "No product category mapping found with the specified id");

            if (ModelState.IsValid)
            {
                if (productCategory == null)
                    return NotFound();

                _productApiService.UpdateProductCategory(product, productCategory);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public IActionResult DeleteProductCategory(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (parameters == null)
                return NotFound();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var categoryId = parameters.FirstOrDefault(x => x.Key == "CategoryId").Value;
            if (categoryId != null)
            {
                var pc = product.Categories.Where(x => x.CategoryId == categoryId.ToString()).FirstOrDefault();
                if (pc == null)
                    ModelState.AddModelError("", "No product category mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    _productApiService.DeleteProductCategory(product, categoryId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pm = product.Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm != null)
                ModelState.AddModelError("", "Product manufacturer mapping found with the specified manufacturerid");

            if (ModelState.IsValid)
            {
                if (productManufacturer == null)
                    return NotFound();

                _productApiService.InsertProductManufacturer(product, productManufacturer);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pm = product.Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm == null)
                ModelState.AddModelError("", "No product manufacturer mapping found with the specified id");

            if (ModelState.IsValid)
            {
                if (productManufacturer == null)
                    return NotFound();

                _productApiService.UpdateProductManufacturer(product, productManufacturer);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public IActionResult DeleteProductManufacturer(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (parameters == null)
                return NotFound();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var manufacturerId = parameters.FirstOrDefault(x => x.Key == "ManufacturerId").Value;
            if (manufacturerId != null)
            {
                var pm = product.Manufacturers.Where(x => x.ManufacturerId == manufacturerId.ToString()).FirstOrDefault();
                if (pm == null)
                    ModelState.AddModelError("", "No product manufacturer mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    _productApiService.DeleteProductManufacturer(product, manufacturerId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }
    }
}
