using Grand.Services.Catalog;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductManufacturerCommandHandler : IRequestHandler<DeleteProductManufacturerCommand, bool>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;

        public DeleteProductManufacturerCommandHandler(IManufacturerService manufacturerService, IProductService productService)
        {
            _manufacturerService = manufacturerService;
            _productService = productService;
        }

        public async Task<bool> Handle(DeleteProductManufacturerCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id, true);
            var productManufacturer = product.ProductManufacturers.Where(x => x.ManufacturerId == request.ManufacturerId).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = product.Id;
            await _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return true;
        }
    }
}
