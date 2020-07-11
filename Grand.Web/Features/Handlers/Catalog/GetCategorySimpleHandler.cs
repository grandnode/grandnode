using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Media;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetCategorySimpleHandler : IRequestHandler<GetCategorySimple, IList<CategorySimpleModel>>
    {
        private readonly ICacheManager _cacheManager;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IMediator _mediator;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;

        public GetCategorySimpleHandler(
            ICacheManager cacheManager, 
            ICategoryService categoryService, 
            IPictureService pictureService, 
            IProductService productService, 
            IMediator mediator, 
            MediaSettings mediaSettings, 
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _categoryService = categoryService;
            _pictureService = pictureService;
            _productService = productService;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
        }

        public async Task<IList<CategorySimpleModel>> Handle(GetCategorySimple request, CancellationToken cancellationToken)
        {
            return await PrepareCategorySimpleModels(request);
        }

        private async Task<List<CategorySimpleModel>> PrepareCategorySimpleModels(GetCategorySimple request)
        {
            string cacheKey = string.Format(ModelCacheEventConst.CATEGORY_ALL_MODEL_KEY,
                request.Language.Id,
                string.Join(",", request.Customer.GetCustomerRoleIds()),
                request.Store.Id);
            return await _cacheManager.GetAsync(cacheKey, () => PrepareCategorySimpleModels(request, ""));
        }

        private async Task<List<CategorySimpleModel>> PrepareCategorySimpleModels(GetCategorySimple request, string rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null)
        {
            var result = new List<CategorySimpleModel>();
            if (allCategories == null)
            {
                allCategories = await _categoryService.GetAllCategories(storeId: request.Store.Id);
            }
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();
            foreach (var category in categories)
            {
                var picture = await _pictureService.GetPictureById(category.PictureId);
                var categoryModel = new CategorySimpleModel {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name, request.Language.Id),
                    SeName = category.GetSeName(request.Language.Id),
                    IncludeInTopMenu = category.IncludeInTopMenu,
                    Flag = category.GetLocalized(x => x.Flag, request.Language.Id),
                    FlagStyle = category.FlagStyle,
                    Icon = category.Icon,
                    ImageUrl = await _pictureService.GetPictureUrl(picture, _mediaSettings.CategoryThumbPictureSize),
                    GenericAttributes = category.GenericAttributes
                };

                //product number for each category
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    var categoryIds = new List<string>();
                    categoryIds.Add(category.Id);
                    //include subcategories
                    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                        categoryIds.AddRange(await _mediator.Send(new GetChildCategoryIds() { Customer = request.Customer, Store = request.Store, ParentCategoryId = category.Id }));
                    categoryModel.NumberOfProducts = _productService.GetCategoryProductNumber(request.Customer, categoryIds, request.Store.Id);
                }
                if (loadSubCategories)
                {
                    var subCategories = await PrepareCategorySimpleModels(request, category.Id, loadSubCategories, allCategories);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }
    }
}
