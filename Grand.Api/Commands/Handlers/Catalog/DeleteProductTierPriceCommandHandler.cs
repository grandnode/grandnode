using Grand.Services.Catalog;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductTierPriceCommandHandler : IRequestHandler<DeleteProductTierPriceCommand, bool>
    {
        private readonly IProductService _productService;

        public DeleteProductTierPriceCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<bool> Handle(DeleteProductTierPriceCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id, true);
            var tierPrice = product.TierPrices.Where(x => x.Id == request.Id).FirstOrDefault();
            tierPrice.ProductId = product.Id;
            await _productService.DeleteTierPrice(tierPrice);

            return true;
        }
    }
}
