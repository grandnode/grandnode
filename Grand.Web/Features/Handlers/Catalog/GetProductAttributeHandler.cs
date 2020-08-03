using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetProductAttributeHandler : IRequestHandler<GetProductAttribute, ProductAttribute>
    {
        private readonly IProductAttributeService _productAttributeService;

        public GetProductAttributeHandler(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        public async Task<ProductAttribute> Handle(GetProductAttribute request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                throw new ArgumentNullException("Id");

            return await _productAttributeService.GetProductAttributeById(request.Id);
        }
    }
}
