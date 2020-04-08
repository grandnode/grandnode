using Grand.Api.Commands.Models.Catalog;
using Grand.Api.DTOs.Catalog;
using Grand.Api.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Handlers.Catalog
{
    public class AddProductAttributeCommandHandler : IRequestHandler<AddProductAttributeCommand, ProductAttributeDto>
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public AddProductAttributeCommandHandler(
            IProductAttributeService productAttributeService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _productAttributeService = productAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task<ProductAttributeDto> Handle(AddProductAttributeCommand request, CancellationToken cancellationToken)
        {
            var productAttribute = request.Model.ToEntity();

            await _productAttributeService.InsertProductAttribute(productAttribute);

            //activity log
            await _customerActivityService.InsertActivity("AddNewProductAttribute", 
                productAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewProductAttribute"), productAttribute.Name);

            return productAttribute.ToModel();
        }
    }
}
