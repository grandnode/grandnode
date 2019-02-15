using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
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

        #region Product category

        [HttpPost]
        public IActionResult CreateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (productCategory == null)
                return NotFound();

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
                _productApiService.InsertProductCategory(product, productCategory);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (productCategory == null)
                return NotFound();

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
                _productApiService.UpdateProductCategory(product, productCategory);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult DeleteProductCategory(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

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

        #endregion

        #region Product manufacturer

        [HttpPost]
        public IActionResult CreateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (productManufacturer == null)
                return NotFound();

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
                _productApiService.InsertProductManufacturer(product, productManufacturer);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (productManufacturer == null)
                return NotFound();

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
                _productApiService.UpdateProductManufacturer(product, productManufacturer);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult DeleteProductManufacturer(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

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

        #endregion

        #region Product picture

        [HttpPost]
        public IActionResult CreateProductPicture(string key, [FromBody] ProductPictureDto productPicture)
        {
            if (productPicture == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pp = product.Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp != null)
                ModelState.AddModelError("", "Product picture mapping found with the specified pictureid");

            if (ModelState.IsValid)
            {
                _productApiService.InsertProductPicture(product, productPicture);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductPicture(string key, [FromBody] ProductPictureDto productPicture)
        {
            if (productPicture == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pp = product.Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp == null)
                ModelState.AddModelError("", "No product picture mapping found with the specified id");

            if (ModelState.IsValid)
            {
                _productApiService.UpdateProductPicture(product, productPicture);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult DeleteProductPicture(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pictureId = parameters.FirstOrDefault(x => x.Key == "PictureId").Value;
            if (pictureId != null)
            {
                var pp = product.Pictures.Where(x => x.PictureId == pictureId.ToString()).FirstOrDefault();
                if (pp == null)
                    ModelState.AddModelError("", "No product picture mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    _productApiService.DeleteProductPicture(product, pictureId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product specification

        [HttpPost]
        public IActionResult CreateProductSpecification(string key, [FromBody] ProductSpecificationAttributeDto productSpecification)
        {
            if (productSpecification == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var psa = product.SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa != null)
                ModelState.AddModelError("", "Product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                _productApiService.InsertProductSpecification(product, productSpecification);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult UpdateProductSpecification(string key, [FromBody] ProductSpecificationAttributeDto productSpecification)
        {
            if (productSpecification == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var psa = product.SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa == null)
                ModelState.AddModelError("", "No product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                _productApiService.UpdateProductSpecification(product, productSpecification);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public IActionResult DeleteProductSpecification(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!_permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var specificationId = parameters.FirstOrDefault(x => x.Key == "Id").Value;
            if (specificationId != null)
            {
                var psa = product.SpecificationAttribute.Where(x => x.Id == specificationId.ToString()).FirstOrDefault();
                if (psa == null)
                    ModelState.AddModelError("", "No product picture specification found with the specified id");

                if (ModelState.IsValid)
                {
                    _productApiService.DeleteProductSpecification(product, specificationId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

    }
}
