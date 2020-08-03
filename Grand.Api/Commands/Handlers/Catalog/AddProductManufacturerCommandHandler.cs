using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductManufacturerCommandHandler : IRequestHandler<AddProductManufacturerCommand, bool>
    {
        private readonly IManufacturerService _manufacturerService;

        public AddProductManufacturerCommandHandler(IManufacturerService manufacturerService)
        {
            _manufacturerService = manufacturerService;
        }

        public async Task<bool> Handle(AddProductManufacturerCommand request, CancellationToken cancellationToken)
        {
            var productManufacturer = new ProductManufacturer {
                ProductId = request.Product.Id,
                ManufacturerId = request.Model.ManufacturerId,
                IsFeaturedProduct = request.Model.IsFeaturedProduct,
            };
            await _manufacturerService.InsertProductManufacturer(productManufacturer);

            return true;
        }
    }
}
