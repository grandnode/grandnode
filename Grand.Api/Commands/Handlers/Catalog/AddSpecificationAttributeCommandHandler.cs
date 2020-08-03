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
    public class AddSpecificationAttributeCommandHandler : IRequestHandler<AddSpecificationAttributeCommand, SpecificationAttributeDto>
    {
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public AddSpecificationAttributeCommandHandler(
            ISpecificationAttributeService specificationAttributeService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _specificationAttributeService = specificationAttributeService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task<SpecificationAttributeDto> Handle(AddSpecificationAttributeCommand request, CancellationToken cancellationToken)
        {
            var specificationAttribute = request.Model.ToEntity();
            await _specificationAttributeService.InsertSpecificationAttribute(specificationAttribute);

            //activity log
            await _customerActivityService.InsertActivity("AddNewSpecAttribute", specificationAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewSpecAttribute"), specificationAttribute.Name);

            return specificationAttribute.ToModel();
        }
    }
}
