using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Vendors;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Queries.Models.Catalog;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Common;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetVendorHandler : IRequestHandler<GetVendor, VendorModel>
    {
        private readonly IMediator _mediator;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        private readonly VendorSettings _vendorSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;

        public GetVendorHandler(
            IMediator mediator,
            IPictureService pictureService,
            ILocalizationService localizationService,
            VendorSettings vendorSettings,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings)
        {
            _mediator = mediator;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _vendorSettings = vendorSettings;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
        }

        public async Task<VendorModel> Handle(GetVendor request, CancellationToken cancellationToken)
        {
            var model = new VendorModel {
                Id = request.Vendor.Id,
                Name = request.Vendor.GetLocalized(x => x.Name, request.Language.Id),
                Description = request.Vendor.GetLocalized(x => x.Description, request.Language.Id),
                MetaKeywords = request.Vendor.GetLocalized(x => x.MetaKeywords, request.Language.Id),
                MetaDescription = request.Vendor.GetLocalized(x => x.MetaDescription, request.Language.Id),
                MetaTitle = request.Vendor.GetLocalized(x => x.MetaTitle, request.Language.Id),
                SeName = request.Vendor.GetSeName(request.Language.Id),
                AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
            };

            model.Address = await _mediator.Send(new GetVendorAddress() {
                Language = request.Language,
                Address = request.Vendor.Address,
                ExcludeProperties = false,
            });

            //prepare picture model
            var pictureModel = new PictureModel {
                Id = request.Vendor.PictureId,
                FullSizeImageUrl = await _pictureService.GetPictureUrl(request.Vendor.PictureId),
                ImageUrl = await _pictureService.GetPictureUrl(request.Vendor.PictureId, _mediaSettings.VendorThumbPictureSize),
                Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), model.Name),
                AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), model.Name)
            };
            model.PictureModel = pictureModel;

            //view/sorting/page size
            var options = await _mediator.Send(new GetViewSortSizeOptions() {
                Command = request.Command,
                PagingFilteringModel = request.Command,
                Language = request.Language,
                AllowCustomersToSelectPageSize = request.Vendor.AllowCustomersToSelectPageSize,
                PageSize = request.Vendor.PageSize,
                PageSizeOptions = request.Vendor.PageSizeOptions
            });
            model.PagingFilteringContext = options.command;

            //products
            var products = (await _mediator.Send(new GetSearchProductsQuery() {
                Customer = request.Customer,
                VendorId = request.Vendor.Id,
                StoreId = request.Store.Id,
                VisibleIndividuallyOnly = true,
                OrderBy = (ProductSortingEnum)request.Command.OrderBy,
                PageIndex = request.Command.PageNumber - 1,
                PageSize = request.Command.PageSize
            })).products;

            model.Products = (await _mediator.Send(new GetProductOverview() {
                Products = products,
                PrepareSpecificationAttributes = _catalogSettings.ShowSpecAttributeOnCatalogPages
            })).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            return model;
        }
    }
}
