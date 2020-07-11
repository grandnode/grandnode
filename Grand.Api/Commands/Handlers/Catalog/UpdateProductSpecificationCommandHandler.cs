using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductSpecificationCommandHandler : IRequestHandler<UpdateProductSpecificationCommand, bool>
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductService _productService;

        public UpdateProductSpecificationCommandHandler(
            ISpecificationAttributeService specificationAttributeService,
            IProductService productService)
        {
            _specificationAttributeService = specificationAttributeService;
            _productService = productService;
        }

        public async Task<bool> Handle(UpdateProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id, true);
            var psa = product.ProductSpecificationAttributes.FirstOrDefault(x => x.Id == request.Model.Id);
            if (psa != null)
            {
                if (request.Model.AttributeType == SpecificationAttributeType.Option)
                {
                    psa.AllowFiltering = request.Model.AllowFiltering;
                    psa.SpecificationAttributeOptionId = request.Model.SpecificationAttributeOptionId;
                }
                else
                {
                    psa.CustomValue = request.Model.CustomValue;
                }
                psa.SpecificationAttributeId = request.Model.SpecificationAttributeId;
                psa.SpecificationAttributeOptionId = request.Model.SpecificationAttributeOptionId;
                psa.AttributeTypeId = (int)request.Model.AttributeType;
                psa.ShowOnProductPage = request.Model.ShowOnProductPage;
                psa.DisplayOrder = request.Model.DisplayOrder;
                psa.ProductId = product.Id;
                await _specificationAttributeService.UpdateProductSpecificationAttribute(psa);
            }

            return true;
        }
    }
}
