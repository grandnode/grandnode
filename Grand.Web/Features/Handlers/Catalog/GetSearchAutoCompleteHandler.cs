using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Media;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Catalog;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetSearchAutoCompleteHandler : IRequestHandler<GetSearchAutoComplete, IList<SearchAutoCompleteModel>>
    {
        private readonly IWebHelper _webHelper;
        private readonly IPictureService _pictureService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ISearchTermService _searchTermService;
        private readonly IBlogService _blogService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMediator _mediator;

        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly BlogSettings _blogSettings;

        public GetSearchAutoCompleteHandler(
            IWebHelper webHelper,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            ICategoryService categoryService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ISearchTermService searchTermService,
            IBlogService blogService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IMediator mediator,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            BlogSettings blogSettings)
        {
            _webHelper = webHelper;
            _pictureService = pictureService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _searchTermService = searchTermService;
            _blogService = blogService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
            _blogSettings = blogSettings;
        }

        public async Task<IList<SearchAutoCompleteModel>> Handle(GetSearchAutoComplete request, CancellationToken cancellationToken)
        {
            var model = new List<SearchAutoCompleteModel>();
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
            var storeId = request.Store.Id;
            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                categoryIds.Add(request.CategoryId);
                if (_catalogSettings.ShowProductsFromSubcategoriesInSearchBox)
                {
                    //include subcategories
                    categoryIds.AddRange(await _mediator.Send(new GetChildCategoryIds() { ParentCategoryId = request.CategoryId, Customer = request.Customer, Store = request.Store }));
                }
            }

            var products = (await _mediator.Send(new GetSearchProductsQuery() {
                Customer = request.Customer,
                StoreId = storeId,
                Keywords = request.Term,
                CategoryIds = categoryIds,
                SearchSku = _catalogSettings.SearchBySku,
                SearchDescriptions = _catalogSettings.SearchByDescription,
                LanguageId = request.Language.Id,
                VisibleIndividuallyOnly = true,
                PageSize = productNumber
            })).products;

            var categories = new List<string>();
            var manufacturers = new List<string>();

            var storeurl = _webHelper.GetStoreLocation();

            foreach (var item in products)
            {
                var pictureUrl = "";
                if (_catalogSettings.ShowProductImagesInSearchAutoComplete)
                {
                    var picture = item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                    if (picture != null)
                        pictureUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.AutoCompleteSearchThumbPictureSize);
                }
                var rating = await _mediator.Send(new GetProductReviewOverview() {
                    Language = request.Language,
                    Product = item,
                    Store = request.Store
                });

                var price = await PreparePrice(item, request);

                model.Add(new SearchAutoCompleteModel() {
                    SearchType = "Product",
                    Label = item.GetLocalized(x => x.Name, request.Language.Id) ?? "",
                    Desc = item.GetLocalized(x => x.ShortDescription, request.Language.Id) ?? "",
                    PictureUrl = pictureUrl,
                    AllowCustomerReviews = rating.AllowCustomerReviews,
                    Rating = rating.TotalReviews > 0 ? (((rating.RatingSum * 100) / rating.TotalReviews) / 5) : 0,
                    Price = price.Price,
                    PriceWithDiscount = price.PriceWithDiscount,
                    Url = $"{storeurl}{item.SeName}"
                });
                foreach (var category in item.ProductCategories)
                {
                    categories.Add(category.CategoryId);
                }
                foreach (var manufacturer in item.ProductManufacturers)
                {
                    manufacturers.Add(manufacturer.ManufacturerId);
                }
            }

            foreach (var item in manufacturers.Distinct())
            {
                var manufacturer = await _manufacturerService.GetManufacturerById(item);
                if (manufacturer != null && manufacturer.Published)
                {
                    var allow = true;
                    if (!_catalogSettings.IgnoreAcl)
                        if (!_aclService.Authorize(manufacturer))
                            allow = false;
                    if (!_catalogSettings.IgnoreStoreLimitations)
                        if (!_storeMappingService.Authorize(manufacturer))
                            allow = false;
                    if (allow)
                    {
                        var desc = "";
                        if (_catalogSettings.SearchByDescription)
                            desc = "&sid=true";
                        model.Add(new SearchAutoCompleteModel() {
                            SearchType = "Manufacturer",
                            Label = manufacturer.GetLocalized(x => x.Name, request.Language.Id),
                            Desc = "",
                            PictureUrl = "",
                            Url = $"{storeurl}search?q={request.Term}&adv=true&mid={item}{desc}"
                        });
                    }
                }
            }
            foreach (var item in categories.Distinct())
            {
                var category = await _categoryService.GetCategoryById(item);
                if (category != null && category.Published)
                {
                    var allow = true;
                    if (!_catalogSettings.IgnoreAcl)
                        if (!_aclService.Authorize(category))
                            allow = false;
                    if (!_catalogSettings.IgnoreStoreLimitations)
                        if (!_storeMappingService.Authorize(category))
                            allow = false;
                    if (allow)
                    {
                        var desc = "";
                        if (_catalogSettings.SearchByDescription)
                            desc = "&sid=true";
                        model.Add(new SearchAutoCompleteModel() {
                            SearchType = "Category",
                            Label = category.GetLocalized(x => x.Name, request.Language.Id),
                            Desc = "",
                            PictureUrl = "",
                            Url = $"{storeurl}search?q={request.Term}&adv=true&cid={item}{desc}"
                        });
                    }
                }
            }

            if (_blogSettings.ShowBlogPostsInSearchAutoComplete)
            {
                var posts = await _blogService.GetAllBlogPosts(storeId: storeId, pageSize: productNumber, blogPostName: request.Term);
                foreach (var item in posts)
                {
                    model.Add(new SearchAutoCompleteModel() {
                        SearchType = "Blog",
                        Label = item.GetLocalized(x => x.Title, request.Language.Id),
                        Desc = "",
                        PictureUrl = "",
                        Url = $"{storeurl}{item.SeName}"
                    });
                }
            }
            //search term statistics
            if (!String.IsNullOrEmpty(request.Term) && _catalogSettings.SaveSearchAutoComplete)
            {
                var searchTerm = await _searchTermService.GetSearchTermByKeyword(request.Term, request.Store.Id);
                if (searchTerm != null)
                {
                    searchTerm.Count++;
                    await _searchTermService.UpdateSearchTerm(searchTerm);
                }
                else
                {
                    searchTerm = new SearchTerm {
                        Keyword = request.Term,
                        StoreId = storeId,
                        Count = 1
                    };
                    await _searchTermService.InsertSearchTerm(searchTerm);
                }

            }
            return model;
        }

        private async Task<(string Price, string PriceWithDiscount)> PreparePrice(Product product, GetSearchAutoComplete request)
        {
            string price, priceWithDiscount;

            decimal finalPriceWithoutDiscountBase =
                (await (_taxService.GetProductPrice(product,
                (await _priceCalculationService.GetFinalPrice(product, request.Customer, includeDiscounts: false)).finalPrice))).productprice;

            var appliedPrice = (await _priceCalculationService.GetFinalPrice(product, request.Customer, includeDiscounts: true));
            var finalPriceWithDiscountBase = (await _taxService.GetProductPrice(product, appliedPrice.finalPrice)).productprice;

            var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, request.Currency);
            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, request.Currency);

            price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
            priceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

            return (price, priceWithDiscount);
        }
    }
}
