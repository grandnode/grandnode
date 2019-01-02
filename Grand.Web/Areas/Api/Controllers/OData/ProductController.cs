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
        //body: { "WarehouseId": "", "Stock": 10 }
        [HttpPost]
        public IActionResult UpdateStock(string key, [FromBody] ODataActionParameters parameters)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Customers))
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
    }
}
