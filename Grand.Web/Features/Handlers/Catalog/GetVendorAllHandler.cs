using Grand.Core.Domain.Media;
using Grand.Core.Domain.Vendors;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Vendors;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Interfaces;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Catalog
{
    public class GetVendorAllHandler : IRequestHandler<GetVendorAll, IList<VendorModel>>
    {
        private readonly IVendorService _vendorService;
        private readonly IAddressViewModelService _addressViewModelService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        public GetVendorAllHandler(
            IVendorService vendorService,
            IAddressViewModelService addressViewModelService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings)
        {
            _vendorService = vendorService;
            _addressViewModelService = addressViewModelService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
        }

        public async Task<IList<VendorModel>> Handle(GetVendorAll request, CancellationToken cancellationToken)
        {
            var model = new List<VendorModel>();

            var vendors = await _vendorService.GetAllVendors();
            foreach (var vendor in vendors)
            {
                var vendorModel = new VendorModel {
                    Id = vendor.Id,
                    Name = vendor.GetLocalized(x => x.Name, request.Language.Id),
                    Description = vendor.GetLocalized(x => x.Description, request.Language.Id),
                    MetaKeywords = vendor.GetLocalized(x => x.MetaKeywords, request.Language.Id),
                    MetaDescription = vendor.GetLocalized(x => x.MetaDescription, request.Language.Id),
                    MetaTitle = vendor.GetLocalized(x => x.MetaTitle, request.Language.Id),
                    SeName = vendor.GetSeName(request.Language.Id),
                    AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors
                };

                //prepare vendor address
                await _addressViewModelService.PrepareVendorAddressModel(model: vendorModel.Address,
                address: vendor.Address,
                excludeProperties: false,
                vendorSettings: _vendorSettings);
                //prepare picture model
                var pictureModel = new PictureModel {
                    Id = vendor.PictureId,
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(vendor.PictureId),
                    ImageUrl = await _pictureService.GetPictureUrl(vendor.PictureId, _mediaSettings.VendorThumbPictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorModel.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorModel.Name)
                };
                vendorModel.PictureModel = pictureModel;
                model.Add(vendorModel);
            }

            return model;
        }
    }
}
