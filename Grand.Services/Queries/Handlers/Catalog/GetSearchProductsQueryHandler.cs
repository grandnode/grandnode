using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Queries.Models.Catalog;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Queries.Handlers.Catalog
{
    public class GetSearchProductsQueryHandler : IRequestHandler<GetSearchProductsQuery, (IPagedList<Product> products, IList<string> filterableSpecificationAttributeOptionIds)>
    {

        private readonly IRepository<Product> _productRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;

        public GetSearchProductsQueryHandler(
            IRepository<Product> productRepository,
            ISpecificationAttributeService specificationAttributeService,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings)
        {
            _productRepository = productRepository;
            _specificationAttributeService = specificationAttributeService;

            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
        }

        public async Task<(IPagedList<Product> products, IList<string> filterableSpecificationAttributeOptionIds)>
            Handle(GetSearchProductsQuery request, CancellationToken cancellationToken)
        {
            var filterableSpecificationAttributeOptionIds = new List<string>();

            //validate "categoryIds" parameter
            if (request.CategoryIds != null && request.CategoryIds.Contains(""))
                request.CategoryIds.Remove("");

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = request.Customer.GetCustomerRoleIds();

            #region Search products

            //products
            var builder = Builders<Product>.Filter;
            var filter = FilterDefinition<Product>.Empty;
            var filterSpecification = FilterDefinition<Product>.Empty;

            //category filtering
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {

                if (request.FeaturedProducts.HasValue)
                {
                    filter = filter & builder.Where(x => x.ProductCategories.Any(y => request.CategoryIds.Contains(y.CategoryId)
                        && y.IsFeaturedProduct == request.FeaturedProducts));
                }
                else
                {
                    filter = filter & builder.Where(x => x.ProductCategories.Any(y => request.CategoryIds.Contains(y.CategoryId)));
                }
            }
            //manufacturer filtering
            if (!string.IsNullOrEmpty(request.ManufacturerId))
            {
                if (request.FeaturedProducts.HasValue)
                {
                    filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == request.ManufacturerId
                        && y.IsFeaturedProduct == request.FeaturedProducts));
                }
                else
                {
                    filter = filter & builder.Where(x => x.ProductManufacturers.Any(y => y.ManufacturerId == request.ManufacturerId));
                }

            }

            if (!request.OverridePublished.HasValue)
            {
                //process according to "showHidden"
                if (!request.ShowHidden)
                {
                    filter = filter & builder.Where(p => p.Published);
                }
            }
            else if (request.OverridePublished.Value)
            {
                //published only
                filter = filter & builder.Where(p => p.Published);
            }
            else if (!request.OverridePublished.Value)
            {
                //unpublished only
                filter = filter & builder.Where(p => !p.Published);
            }
            if (request.VisibleIndividuallyOnly)
            {
                filter = filter & builder.Where(p => p.VisibleIndividually);
            }
            if (request.ProductType.HasValue)
            {
                var productTypeId = (int)request.ProductType.Value;
                filter = filter & builder.Where(p => p.ProductTypeId == productTypeId);
            }

            //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
            //That's why we pass the date value
            var nowUtc = DateTime.UtcNow;
            if (request.PriceMin.HasValue)
            {
                filter = filter & builder.Where(p => p.Price >= request.PriceMin.Value);
            }
            if (request.PriceMax.HasValue)
            {
                //max price
                filter = filter & builder.Where(p => p.Price <= request.PriceMax.Value);
            }
            if (!request.ShowHidden && !_catalogSettings.IgnoreFilterableAvailableStartEndDateTime)
            {
                filter = filter & builder.Where(p =>
                    (p.AvailableStartDateTimeUtc == null || p.AvailableStartDateTimeUtc < nowUtc) &&
                    (p.AvailableEndDateTimeUtc == null || p.AvailableEndDateTimeUtc > nowUtc));


            }

            if (request.MarkedAsNewOnly)
            {
                filter = filter & builder.Where(p => p.MarkAsNew);
                filter = filter & builder.Where(p =>
                    (!p.MarkAsNewStartDateTimeUtc.HasValue || p.MarkAsNewStartDateTimeUtc.Value < nowUtc) &&
                    (!p.MarkAsNewEndDateTimeUtc.HasValue || p.MarkAsNewEndDateTimeUtc.Value > nowUtc));
            }

            //searching by keyword
            if (!String.IsNullOrWhiteSpace(request.Keywords))
            {
                if (_commonSettings.UseFullTextSearch)
                {
                    request.Keywords = "\"" + request.Keywords + "\"";
                    request.Keywords = request.Keywords.Replace("+", "\" \"");
                    request.Keywords = request.Keywords.Replace(" ", "\" \"");
                    filter = filter & builder.Text(request.Keywords);
                }
                else
                {
                    if (!request.SearchDescriptions)
                        filter = filter & builder.Where(p =>
                            p.Name.ToLower().Contains(request.Keywords.ToLower())
                            ||
                            p.Locales.Any(x => x.LocaleKey == "Name" && x.LocaleValue != null && x.LocaleValue.ToLower().Contains(request.Keywords.ToLower()))
                            ||
                            (request.SearchSku && p.Sku.ToLower().Contains(request.Keywords.ToLower()))
                            );
                    else
                    {
                        filter = filter & builder.Where(p =>
                                (p.Name != null && p.Name.ToLower().Contains(request.Keywords.ToLower()))
                                ||
                                (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(request.Keywords.ToLower()))
                                ||
                                (p.FullDescription != null && p.FullDescription.ToLower().Contains(request.Keywords.ToLower()))
                                ||
                                (p.Locales.Any(x => x.LocaleValue != null && x.LocaleValue.ToLower().Contains(request.Keywords.ToLower())))
                                ||
                                (request.SearchSku && p.Sku.ToLower().Contains(request.Keywords.ToLower()))
                                );
                    }
                }

            }

            if (!request.ShowHidden && !_catalogSettings.IgnoreAcl)
            {
                filter = filter & (builder.AnyIn(x => x.CustomerRoles, allowedCustomerRolesIds) | builder.Where(x => !x.SubjectToAcl));
            }

            if (!string.IsNullOrEmpty(request.StoreId) && !_catalogSettings.IgnoreStoreLimitations)
            {
                filter = filter & builder.Where(x => x.Stores.Any(y => y == request.StoreId) || !x.LimitedToStores);

            }
            //vendor filtering
            if (!string.IsNullOrEmpty(request.VendorId))
            {
                filter = filter & builder.Where(x => x.VendorId == request.VendorId);
            }
            //warehouse filtering
            if (!string.IsNullOrEmpty(request.WarehouseId))
            {
                filter = filter & (builder.Where(x => x.UseMultipleWarehouses && x.ProductWarehouseInventory.Any(y => y.WarehouseId == request.WarehouseId)) |
                    builder.Where(x => !x.UseMultipleWarehouses && x.WarehouseId == request.WarehouseId));

            }

            //tag filtering
            if (!string.IsNullOrEmpty(request.ProductTag))
            {
                filter = filter & builder.Where(x => x.ProductTags.Any(y => y == request.ProductTag));
            }

            filterSpecification = filter;

            //search by specs
            if (request.FilteredSpecs != null && request.FilteredSpecs.Any())
            {
                var spec = new HashSet<string>();
                Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                foreach (string key in request.FilteredSpecs)
                {
                    var specification = await _specificationAttributeService.GetSpecificationAttributeByOptionId(key);
                    if (specification != null)
                    {
                        spec.Add(specification.Id);
                        if (!dictionary.ContainsKey(specification.Id))
                        {
                            //add
                            dictionary.Add(specification.Id, new List<string>());
                            filterSpecification = filterSpecification & builder.Where(x => x.ProductSpecificationAttributes.Any(y => y.SpecificationAttributeId == specification.Id && y.AllowFiltering));
                        }
                        dictionary[specification.Id].Add(key);
                    }
                }

                foreach (var item in dictionary)
                {
                    filter = filter & builder.Where(x => x.ProductSpecificationAttributes.Any(y => y.SpecificationAttributeId == item.Key && y.AllowFiltering
                    && item.Value.Contains(y.SpecificationAttributeOptionId)));
                }
            }

            var builderSort = Builders<Product>.Sort.Descending(x => x.CreatedOnUtc);

            if (request.OrderBy == ProductSortingEnum.Position && request.CategoryIds != null && request.CategoryIds.Any())
            {
                //category position
                builderSort = Builders<Product>.Sort.Ascending(x => x.DisplayOrderCategory);
            }
            else if (request.OrderBy == ProductSortingEnum.Position && !String.IsNullOrEmpty(request.ManufacturerId))
            {
                //manufacturer position
                builderSort = Builders<Product>.Sort.Ascending(x => x.DisplayOrderManufacturer);
            }
            else if (request.OrderBy == ProductSortingEnum.Position)
            {
                //otherwise sort by name
                builderSort = Builders<Product>.Sort.Ascending(x => x.Name);
            }
            else if (request.OrderBy == ProductSortingEnum.NameAsc)
            {
                //Name: A to Z
                builderSort = Builders<Product>.Sort.Ascending(x => x.Name);
            }
            else if (request.OrderBy == ProductSortingEnum.NameDesc)
            {
                //Name: Z to A
                builderSort = Builders<Product>.Sort.Descending(x => x.Name);
            }
            else if (request.OrderBy == ProductSortingEnum.PriceAsc)
            {
                //Price: Low to High
                builderSort = Builders<Product>.Sort.Ascending(x => x.Price);
            }
            else if (request.OrderBy == ProductSortingEnum.PriceDesc)
            {
                //Price: High to Low
                builderSort = Builders<Product>.Sort.Descending(x => x.Price);
            }
            else if (request.OrderBy == ProductSortingEnum.CreatedOn)
            {
                //creation date
                builderSort = Builders<Product>.Sort.Ascending(x => x.CreatedOnUtc);
            }
            else if (request.OrderBy == ProductSortingEnum.OnSale)
            {
                //on sale
                builderSort = Builders<Product>.Sort.Descending(x => x.OnSale);
            }
            else if (request.OrderBy == ProductSortingEnum.MostViewed)
            {
                //most viewed
                builderSort = Builders<Product>.Sort.Descending(x => x.Viewed);
            }
            else if (request.OrderBy == ProductSortingEnum.BestSellers)
            {
                //best seller
                builderSort = Builders<Product>.Sort.Descending(x => x.Sold);
            }

            var products = await PagedList<Product>.Create(_productRepository.Collection, filter, builderSort, request.PageIndex, request.PageSize);

            if (request.LoadFilterableSpecificationAttributeOptionIds && !_catalogSettings.IgnoreFilterableSpecAttributeOption)
            {
                IList<string> specyfication = new List<string>();
                var filterSpecExists = filterSpecification &
                    builder.Where(x => x.ProductSpecificationAttributes.Count > 0);
                var productSpec = _productRepository.Collection.Find(filterSpecExists).Limit(1);
                if (productSpec != null)
                {
                    var qspec = await _productRepository.Collection
                    .Aggregate()
                    .Match(filterSpecification)
                    .Unwind(x => x.ProductSpecificationAttributes)
                    .Project(new BsonDocument
                        {
                        {"AllowFiltering", "$ProductSpecificationAttributes.AllowFiltering"},
                        {"SpecificationAttributeOptionId", "$ProductSpecificationAttributes.SpecificationAttributeOptionId"}
                        })
                    .Match(new BsonDocument("AllowFiltering", true))
                    .Group(new BsonDocument
                            {
                                        {"_id",
                                            new BsonDocument {
                                                { "SpecificationAttributeOptionId", "$SpecificationAttributeOptionId" },
                                            }
                                        },
                                        {"count", new BsonDocument
                                            {
                                                { "$sum" , 1}
                                            }
                                        }
                            })
                    .ToListAsync();
                    foreach (var item in qspec)
                    {
                        var so = item["_id"]["SpecificationAttributeOptionId"].ToString();
                        specyfication.Add(so);
                    }
                }

                filterableSpecificationAttributeOptionIds = specyfication.ToList();
            }

            return (products, filterableSpecificationAttributeOptionIds);

            #endregion
        }
    }
}
