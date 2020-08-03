using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class AddProductSpecificationCommandHandler : IRequestHandler<AddProductSpecificationCommand, bool>
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public AddProductSpecificationCommandHandler(ISpecificationAttributeService specificationAttributeService)
        {
            _specificationAttributeService = specificationAttributeService;
        }

        public async Task<bool> Handle(AddProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            //we allow filtering only for "Option" attribute type
            if (request.Model.AttributeType != SpecificationAttributeType.Option)
            {
                request.Model.AllowFiltering = false;
                request.Model.SpecificationAttributeOptionId = null;
            }

            var psa = new ProductSpecificationAttribute {
                AttributeTypeId = (int)request.Model.AttributeType,
                SpecificationAttributeOptionId = request.Model.SpecificationAttributeOptionId,
                SpecificationAttributeId = request.Model.SpecificationAttributeId,
                ProductId = request.Product.Id,
                CustomValue = request.Model.CustomValue,
                AllowFiltering = request.Model.AllowFiltering,
                ShowOnProductPage = request.Model.ShowOnProductPage,
                DisplayOrder = request.Model.DisplayOrder,
            };
            await _specificationAttributeService.InsertProductSpecificationAttribute(psa);

            return true;
        }
    }
}
