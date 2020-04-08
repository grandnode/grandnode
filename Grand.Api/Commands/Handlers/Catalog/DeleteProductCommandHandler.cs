using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductService _productService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public DeleteProductCommandHandler(
            IProductService productService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _productService = productService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Model.Id);
            if (product != null)
            {
                await _productService.DeleteProduct(product);

                //activity log
                await _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);
            }
            return true;
        }
    }
}
