using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Topics;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetTopMenuHandler : IRequestHandler<GetTopMenu, TopMenuModel>
    {
        private readonly IMediator _mediator;
        private readonly ITopicService _topicService;
        private readonly ICacheBase _cacheBase;
        private readonly IManufacturerService _manufacturerService;
        private readonly CatalogSettings _catalogSettings;
        private readonly BlogSettings _blogSettings;
        private readonly MenuItemSettings _menuItemSettings;

        public GetTopMenuHandler(
            IMediator mediator,
            ITopicService topicService,
            ICacheBase cacheManager,
            IManufacturerService manufacturerService,
            CatalogSettings catalogSettings,
            BlogSettings blogSettings,
            MenuItemSettings menuItemSettings)
        {
            _mediator = mediator;
            _topicService = topicService;
            _cacheBase = cacheManager;
            _manufacturerService = manufacturerService;
            _catalogSettings = catalogSettings;
            _blogSettings = blogSettings;
            _menuItemSettings = menuItemSettings;
        }

        public async Task<TopMenuModel> Handle(GetTopMenu request, CancellationToken cancellationToken)
        {
            //categories
            var cachedCategoriesModel = await _mediator.Send(new GetCategorySimple() { Customer = request.Customer, Language = request.Language, Store = request.Store });

            //top menu topics
            var now = DateTime.UtcNow;
            var topicModel = (await _topicService.GetAllTopics(request.Store.Id))
                .Where(t => t.IncludeInTopMenu && (!t.StartDateUtc.HasValue || t.StartDateUtc < now) && (!t.EndDateUtc.HasValue || t.EndDateUtc > now))
                .Select(t => new TopMenuModel.TopMenuTopicModel {
                    Id = t.Id,
                    Name = t.GetLocalized(x => x.Title, request.Language.Id),
                    SeName = t.GetSeName(request.Language.Id)
                }).ToList();

            string manufacturerCacheKey = string.Format(ModelCacheEventConst.MANUFACTURER_NAVIGATION_MENU,
                request.Language.Id, request.Store.Id);

            var cachedManufacturerModel = await _cacheBase.GetAsync(manufacturerCacheKey, async () =>
                    (await _manufacturerService.GetAllManufacturers(storeId: request.Store.Id))
                    .Where(x => x.IncludeInTopMenu)
                    .Select(t => new TopMenuModel.TopMenuManufacturerModel {
                        Id = t.Id,
                        Name = t.GetLocalized(x => x.Name, request.Language.Id),
                        Icon = t.Icon,
                        SeName = t.GetSeName(request.Language.Id)
                    })
                    .ToList()
                );

            var model = new TopMenuModel {
                Categories = cachedCategoriesModel,
                Topics = topicModel,
                Manufacturers = cachedManufacturerModel,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                DisplayHomePageMenu = _menuItemSettings.DisplayHomePageMenu,
                DisplayNewProductsMenu = _menuItemSettings.DisplayNewProductsMenu,
                DisplaySearchMenu = _menuItemSettings.DisplaySearchMenu,
                DisplayCustomerMenu = _menuItemSettings.DisplayCustomerMenu,
                DisplayBlogMenu = _menuItemSettings.DisplayBlogMenu,
                DisplayContactUsMenu = _menuItemSettings.DisplayContactUsMenu
            };

            return model;

        }
    }
}
