using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductSpecificationHandler : IRequestHandler<GetProductSpecification, IList<ProductSpecificationModel>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        public GetProductSpecificationHandler(ICacheManager cacheManager, ISpecificationAttributeService specificationAttributeService)
        {
            _cacheManager = cacheManager;
            _specificationAttributeService = specificationAttributeService;
        }

        public async Task<IList<ProductSpecificationModel>> Handle(GetProductSpecification request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConst.PRODUCT_SPECS_MODEL_KEY, request.Product.Id, request.Language.Id);
            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var spa = new List<ProductSpecificationModel>();
                foreach (var item in request.Product.ProductSpecificationAttributes.Where(x => x.ShowOnProductPage).OrderBy(x => x.DisplayOrder))
                {
                    var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(item.SpecificationAttributeId);
                    var m = new ProductSpecificationModel {
                        SpecificationAttributeId = item.SpecificationAttributeId,
                        SpecificationAttributeName = specificationAttribute.GetLocalized(x => x.Name, request.Language.Id),
                        ColorSquaresRgb = specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault() != null ? specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault().ColorSquaresRgb : "",
                        GenericAttributes = specificationAttribute.GenericAttributes,
                    };

                    switch (item.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault().GetLocalized(x => x.Name, request.Language.Id));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(item.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = item.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", item.CustomValue);
                            break;
                        default:
                            break;
                    }
                    spa.Add(m);

                }
                return spa;
            }

            );
        }
    }
}
