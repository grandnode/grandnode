using Grand.Api.Extensions;
using Grand.Services.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteProductAttributeMappingCommandHandler : IRequestHandler<DeleteProductAttributeMappingCommand, bool>
    {
        private readonly IProductAttributeService _productAttributeService;

        public DeleteProductAttributeMappingCommandHandler(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        public async Task<bool> Handle(DeleteProductAttributeMappingCommand request, CancellationToken cancellationToken)
        {
            //insert mapping
            var productAttributeMapping = request.Model.ToEntity();
            productAttributeMapping.ProductId = request.Product.Id;
            await _productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);

            return true;
        }
    }
}
