using Grand.Api.Commands.Models.Catalog;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Queries.Models.Common;
using Grand.Services.Security;
using MediatR;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class ProductController : BaseODataController
    {
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;

        public ProductController(
            IMediator mediator,
            IPermissionService permissionService)
        {
            _mediator = mediator;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            return Ok(product.FirstOrDefault());
        }

        [HttpGet]
        [EnableQuery(HandleNullPropagation = HandleNullPropagationOption.False)]
        public async Task<IActionResult> Get()
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            return Ok(await _mediator.Send(new GetQuery<ProductDto>()));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            if (ModelState.IsValid)
            {
                model = await _mediator.Send(new AddProductCommand() { Model = model });
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
                await _mediator.Send(new UpdateProductCommand() { Model = model });
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] string key, Delta<ProductDto> model)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pr = product.FirstOrDefault();
            model.Patch(pr);

            if (ModelState.IsValid)
            {
                await _mediator.Send(new UpdateProductCommand() { Model = pr });
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string key)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            await _mediator.Send(new DeleteProductCommand() { Model = product.FirstOrDefault() });

            return Ok();
        }

        //odata/Product(id)/UpdateStock
        //body: { "Stock": 10 }
        [HttpPost]
        public async Task<IActionResult> UpdateStock(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            if (parameters == null)
                return BadRequest();

            var warehouseId = parameters.FirstOrDefault(x => x.Key == "WarehouseId").Value;
            var stock = parameters.FirstOrDefault(x => x.Key == "Stock").Value;
            if (stock != null)
            {
                if (int.TryParse(stock.ToString(), out int stockqty))
                {
                    await _mediator.Send(new UpdateProductStockCommand() { Product = product.FirstOrDefault(), WarehouseId = warehouseId?.ToString(), Stock = stockqty });
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
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pc = product.FirstOrDefault().Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc != null)
                ModelState.AddModelError("", "Product category mapping found with the specified categoryid");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductCategoryCommand() { Product = product.FirstOrDefault(), Model = productCategory });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductCategory(string key, [FromBody] ProductCategoryDto productCategory)
        {
            if (productCategory == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var pc = product.FirstOrDefault().Categories.Where(x => x.CategoryId == productCategory.CategoryId).FirstOrDefault();
            if (pc == null)
                ModelState.AddModelError("", "No product category mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductCategoryCommand() { Product = product.FirstOrDefault(), Model = productCategory });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductCategory(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var categoryId = parameters.FirstOrDefault(x => x.Key == "CategoryId").Value;
            if (categoryId != null)
            {
                var pc = product.FirstOrDefault().Categories.Where(x => x.CategoryId == categoryId.ToString()).FirstOrDefault();
                if (pc == null)
                    ModelState.AddModelError("", "No product category mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductCategoryCommand() { Product = product.FirstOrDefault(), CategoryId = categoryId.ToString() });
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
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var pm = product.FirstOrDefault().Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm != null)
                ModelState.AddModelError("", "Product manufacturer mapping found with the specified manufacturerid");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductManufacturerCommand() { Product = product.FirstOrDefault(), Model = productManufacturer });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductManufacturer(string key, [FromBody] ProductManufacturerDto productManufacturer)
        {
            if (productManufacturer == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pm = product.FirstOrDefault().Manufacturers.Where(x => x.ManufacturerId == productManufacturer.ManufacturerId).FirstOrDefault();
            if (pm == null)
                ModelState.AddModelError("", "No product manufacturer mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductManufacturerCommand() { Product = product.FirstOrDefault(), Model = productManufacturer });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductManufacturer(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var manufacturerId = parameters.FirstOrDefault(x => x.Key == "ManufacturerId").Value;
            if (manufacturerId != null)
            {
                var pm = product.FirstOrDefault().Manufacturers.Where(x => x.ManufacturerId == manufacturerId.ToString()).FirstOrDefault();
                if (pm == null)
                    ModelState.AddModelError("", "No product manufacturer mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductManufacturerCommand() { Product = product.FirstOrDefault(), ManufacturerId = manufacturerId.ToString() });
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
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var pp = product.FirstOrDefault().Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp != null)
                ModelState.AddModelError("", "Product picture mapping found with the specified pictureid");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductPictureCommand() { Product = product.FirstOrDefault(), Model = productPicture });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductPicture(string key, [FromBody] ProductPictureDto productPicture)
        {
            if (productPicture == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var pp = product.FirstOrDefault().Pictures.Where(x => x.PictureId == productPicture.PictureId).FirstOrDefault();
            if (pp == null)
                ModelState.AddModelError("", "No product picture mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductPictureCommand() { Product = product.FirstOrDefault(), Model = productPicture });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductPicture(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var pictureId = parameters.FirstOrDefault(x => x.Key == "PictureId").Value;
            if (pictureId != null)
            {
                var pp = product.FirstOrDefault().Pictures.Where(x => x.PictureId == pictureId.ToString()).FirstOrDefault();
                if (pp == null)
                    ModelState.AddModelError("", "No product picture mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductPictureCommand() { Product = product.FirstOrDefault(), PictureId = pictureId.ToString() });
                    return Ok(result);
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
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var psa = product.FirstOrDefault().SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa != null)
                ModelState.AddModelError("", "Product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductSpecificationCommand() { Product = product.FirstOrDefault(), Model = productSpecification });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductSpecification(string key, [FromBody] ProductSpecificationAttributeDto productSpecification)
        {
            if (productSpecification == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var psa = product.FirstOrDefault().SpecificationAttribute.Where(x => x.Id == productSpecification.Id).FirstOrDefault();
            if (psa == null)
                ModelState.AddModelError("", "No product specification mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductSpecificationCommand() { Product = product.FirstOrDefault(), Model = productSpecification });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductSpecification(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var specificationId = parameters.FirstOrDefault(x => x.Key == "Id").Value;
            if (specificationId != null)
            {
                var psa = product.FirstOrDefault().SpecificationAttribute.Where(x => x.Id == specificationId.ToString()).FirstOrDefault();
                if (psa == null)
                    ModelState.AddModelError("", "No product specification mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductSpecificationCommand() { Product = product.FirstOrDefault(), Id = specificationId.ToString() });
                    return Ok(result);
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
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pt = product.FirstOrDefault().TierPrices.Where(x => x.Id == productTierPrice.Id).FirstOrDefault();
            if (pt != null)
                ModelState.AddModelError("", "Product tier price mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductTierPriceCommand() { Product = product.FirstOrDefault(), Model = productTierPrice });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProductTierPrice(string key, [FromBody] ProductTierPriceDto productTierPrice)
        {
            if (productTierPrice == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pt = product.FirstOrDefault().TierPrices.Where(x => x.Id == productTierPrice.Id).FirstOrDefault();
            if (pt == null)
                ModelState.AddModelError("", "No product tier price mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductTierPriceCommand() { Product = product.FirstOrDefault(), Model = productTierPrice });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductTierPrice(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var tierPriceId = parameters.FirstOrDefault(x => x.Key == "Id").Value;
            if (tierPriceId != null)
            {
                var pt = product.FirstOrDefault().TierPrices.Where(x => x.Id == tierPriceId.ToString()).FirstOrDefault();
                if (pt == null)
                    ModelState.AddModelError("", "No product tier price mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductTierPriceCommand() { Product = product.FirstOrDefault(), Id = tierPriceId.ToString() });
                    return Ok(result);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }

        #endregion

        #region Product attribute mapping

        [HttpPost]
        public async Task<IActionResult> CreateProductAttributeMapping(string key, [FromBody] ProductAttributeMappingDto productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pam = product.FirstOrDefault().AttributeMappings.Where(x => x.Id == productAttributeMapping.Id).FirstOrDefault();
            if (pam != null)
                ModelState.AddModelError("", "Product attribute mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new AddProductAttributeMappingCommand() { Product = product.FirstOrDefault(), Model = productAttributeMapping });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductAttributeMapping(string key, [FromBody] ProductAttributeMappingDto productAttributeMapping)
        {
            if (productAttributeMapping == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();

            var pam = product.FirstOrDefault().AttributeMappings.Where(x => x.Id == productAttributeMapping.Id).FirstOrDefault();
            if (pam == null)
                ModelState.AddModelError("", "No product attribute mapping found with the specified id");

            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(new UpdateProductAttributeMappingCommand() { Product = product.FirstOrDefault(), Model = productAttributeMapping });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductAttributeMapping(string key, [FromBody] ODataActionParameters parameters)
        {
            if (parameters == null)
                return BadRequest();

            if (!await _permissionService.Authorize(PermissionSystemName.Products))
                return Forbid();

            var product = await _mediator.Send(new GetQuery<ProductDto>() { Id = key });
            if (!product.Any())
                return NotFound();


            var attrId = parameters.FirstOrDefault(x => x.Key == "Id").Value;
            if (attrId != null)
            {
                var pam = product.FirstOrDefault().AttributeMappings.Where(x => x.Id == attrId.ToString()).FirstOrDefault();
                if (pam == null)
                    ModelState.AddModelError("", "No product attribute mapping found with the specified id");

                if (ModelState.IsValid)
                {
                    var result = await _mediator.Send(new DeleteProductAttributeMappingCommand() { Product = product.FirstOrDefault(),  Model = pam });
                    return Ok(result);
                }
                return BadRequest(ModelState);
            }
            return NotFound();
        }
        #endregion
    }
}
