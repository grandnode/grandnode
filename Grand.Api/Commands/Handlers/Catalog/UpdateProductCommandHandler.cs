using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Domain.Seo;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Seo;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSettings;

        public UpdateProductCommandHandler(
            IProductService productService, 
            IUrlRecordService urlRecordService, 
            ICustomerActivityService customerActivityService, 
            ILocalizationService localizationService, 
            ILanguageService languageService, 
            SeoSettings seoSettings)
        {
            _productService = productService;
            _urlRecordService = urlRecordService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
            _seoSettings = seoSettings;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            //product
            var product = await _productService.GetProductById(request.Model.Id);
            product = request.Model.ToEntity(product);
            product.UpdatedOnUtc = DateTime.UtcNow;
            request.Model.SeName = await product.ValidateSeName(request.Model.SeName, product.Name, true, _seoSettings, _urlRecordService, _languageService);
            product.SeName = request.Model.SeName;
            //search engine name
            await _urlRecordService.SaveSlug(product, request.Model.SeName, "");
            //update product
            await _productService.UpdateProduct(product);

            //activity log
            await _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);

            return product.ToModel();
        }
    }
}
