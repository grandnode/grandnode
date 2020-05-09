using Grand.Services.Catalog;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductManufacturerCommandHandler : IRequestHandler<UpdateProductManufacturerCommand, bool>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;

        public UpdateProductManufacturerCommandHandler(IManufacturerService manufacturerService, IProductService productService)
        {
            _manufacturerService = manufacturerService;
            _productService = productService;
        }

        public async Task<bool> Handle(UpdateProductManufacturerCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id, true);
            var productManufacturer = product.ProductManufacturers.Where(x => x.ManufacturerId == request.Model.ManufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ManufacturerId = request.Model.ManufacturerId;
            productManufacturer.ProductId = product.Id;
            productManufacturer.IsFeaturedProduct = request.Model.IsFeaturedProduct;

            await _manufacturerService.UpdateProductManufacturer(productManufacturer);

            return true;
        }
    }
}
