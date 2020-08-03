using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetSearchProductsQuery : IRequest<(IPagedList<Product> products, IList<string> filterableSpecificationAttributeOptionIds)>
    {
        public Customer Customer { get; set; }

        public bool LoadFilterableSpecificationAttributeOptionIds { get; set; } = false;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = int.MaxValue;
        public IList<string> CategoryIds { get; set; } = null;
        public string ManufacturerId { get; set; } = "";
        public string StoreId { get; set; } = "";
        public string VendorId { get; set; } = "";
        public string WarehouseId { get; set; } = "";
        public ProductType? ProductType { get; set; } = null;
        public bool VisibleIndividuallyOnly { get; set; } = false;
        public bool MarkedAsNewOnly { get; set; } = false;
        public bool? FeaturedProducts { get; set; } = null;
        public decimal? PriceMin { get; set; } = null;
        public decimal? PriceMax { get; set; } = null;
        public string ProductTag { get; set; } = "";
        public string Keywords { get; set; } = null;
        public bool SearchDescriptions { get; set; } = false;
        public bool SearchSku { get; set; } = true;
        public bool SearchProductTags { get; set; } = false;
        public string LanguageId { get; set; } = "";
        public IList<string> FilteredSpecs { get; set; } = null;
        public ProductSortingEnum OrderBy { get; set; } = ProductSortingEnum.Position;
        public bool ShowHidden { get; set; } = false;
        public bool? OverridePublished { get; set; } = null;
    }
}
