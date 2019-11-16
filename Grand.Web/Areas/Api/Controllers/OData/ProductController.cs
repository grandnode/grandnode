using Grand.Api.Controllers;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Interfaces;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            return Ok(_productApiService.GetProducts());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _productApiService.InsertOrUpdateProduct(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProductDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _productApiService.UpdateProduct(model);
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<ProductDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var entity = await _productApiService.GetById(key);
            if (entity == null)
            {
                return NotFound();
            }
            model.Patch(entity);

            if (ModelState.IsValid)
            {
                entity = await _productApiService.UpdateProduct(entity);
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }
            await _productApiService.DeleteProduct(product);

            return Ok();
        }

        //odata/Product(id)/UpdateStock
        //body: { "Stock": 10 }
        [HttpPost]
        public async Task<IActionResult> UpdateStock(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
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
                    await _productApiService.UpdateStock(product, warehouseId?.ToString(), stockqty);
                    return Ok(true);
                }
            }
            return Ok(false);
        }

        #region Product category

        [HttpPost]
        public async Task<IActionResult> CreateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (productCategory == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pc = product.Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc != null)
                ModelState.AddModelError("", "Product category mapping found with the specified categoryid");

            if (ModelState.IsValid)
            {
                await _productApiService.InsertProductCategory(product, productCategory);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (productCategory == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pc = product.Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc == null)
                ModelState.AddModelError("", "No product category mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.UpdateProductCategory(product, productCategory);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductCategory(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
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
                    await _productApiService.DeleteProductCategory(product, categoryId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product manufacturer

        [HttpPost]
        public async Task<IActionResult> CreateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (productManufacturer == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pm = product.Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm != null)
                ModelState.AddModelError("", "Product manufacturer mapping found with the specified manufacturerid");

            if (ModelState.IsValid)
            {
                await _productApiService.InsertProductManufacturer(product, productManufacturer);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (productManufacturer == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pm = product.Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm == null)
                ModelState.AddModelError("", "No product manufacturer mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.UpdateProductManufacturer(product, productManufacturer);

                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductManufacturer(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
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
                    await _productApiService.DeleteProductManufacturer(product, manufacturerId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product picture

        [HttpPost]
        public async Task<IActionResult> CreateProductPicture(string key, [FromBody] ProductPictureDto productPicture)
        {
            if (productPicture == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pp = product.Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp != null)
                ModelState.AddModelError("", "Product picture mapping found with the specified pictureid");

            if (ModelState.IsValid)
            {
                await _productApiService.InsertProductPicture(product, productPicture);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductPicture(string key, [FromBody] ProductPictureDto productPicture)
        {
            if (productPicture == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pp = product.Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp == null)
                ModelState.AddModelError("", "No product picture mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.UpdateProductPicture(product, productPicture);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductPicture(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
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
                    await _productApiService.DeleteProductPicture(product, pictureId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product specification

        [HttpPost]
        public async Task<IActionResult> CreateProductSpecification(string key, [FromBody] ProductSpecificationAttributeDto productSpecification)
        {
            if (productSpecification == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var psa = product.SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa != null)
                ModelState.AddModelError("", "Product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.InsertProductSpecification(product, productSpecification);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductSpecification(string key, [FromBody] ProductSpecificationAttributeDto productSpecification)
        {
            if (productSpecification == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var psa = product.SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa == null)
                ModelState.AddModelError("", "No product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.UpdateProductSpecification(product, productSpecification);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductSpecification(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
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
                    await _productApiService.DeleteProductSpecification(product, specificationId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product tierprice

        [HttpPost]
        public async Task<IActionResult> CreateProductTierPrice(string key, [FromBody] ProductTierPriceDto productTierPrice)
        {
            if (productTierPrice == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pt = product.TierPrices.Where(x => x.Id == productTierPrice.Id).FirstOrDefault();
            if (pt != null)
                ModelState.AddModelError("", "Product tier price mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.InsertProductTierPrice(product, productTierPrice);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductTierPrice(string key, [FromBody] ProductTierPriceDto productTierPrice)
        {
            if (productTierPrice == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var pt = product.TierPrices.Where(x => x.Id == productTierPrice.Id).FirstOrDefault();
            if (pt == null)
                ModelState.AddModelError("", "No product tier price mapping found with the specified id");

            if (ModelState.IsValid)
            {
                await _productApiService.UpdateProductTierPrice(product, productTierPrice);
                return Ok(true);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductTierPrice(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return NotFound();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _productApiService.GetById(key);
            if (product == null)
            {
                return NotFound();
            }

            var tierPriceId = parameters.FirstOrDefault(x => x.Key == "Id").Value;
            if (tierPriceId != null)
            {
                var pt = product.TierPrices.Where(x => x.Id == tierPriceId.ToString()).FirstOrDefault();
                if (pt == null)
                    ModelState.AddModelError("", "No product tier price mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    await _productApiService.DeleteProductTierPrice(product, tierPriceId.ToString());
                    return Ok(true);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion
    }
}
