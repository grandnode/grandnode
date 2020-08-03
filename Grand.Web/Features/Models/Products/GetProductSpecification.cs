using Grand.Domain.Catalog;
using Grand.Domain.Localization;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductSpecification : IRequest<IList<ProductSpecificationModel>>
    {
        public Product Product { get; set; }
        public Language Language { get; set; }

    }
}
