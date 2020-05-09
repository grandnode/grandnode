using Grand.Api.Extensions;
using Grand.Services.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductTierPriceCommandHandler : IRequestHandler<AddProductTierPriceCommand, bool>
    {
        private readonly IProductService _productService;

        public AddProductTierPriceCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<bool> Handle(AddProductTierPriceCommand request, CancellationToken cancellationToken)
        {
            var tierPrice = request.Model.ToEntity();
            tierPrice.ProductId = request.Product.Id;
            await _productService.InsertTierPrice(tierPrice);

            return true;
        }
    }
}
