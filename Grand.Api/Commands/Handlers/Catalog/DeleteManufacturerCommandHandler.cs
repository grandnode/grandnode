using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Logging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class DeleteManufacturerCommandHandler : IRequestHandler<DeleteManufacturerCommand, bool>
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public DeleteManufacturerCommandHandler(
            IManufacturerService manufacturerService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _manufacturerService = manufacturerService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task<bool> Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufacturerService.GetManufacturerById(request.Model.Id);
            if (manufacturer != null)
            {
                await _manufacturerService.DeleteManufacturer(manufacturer);

                //activity log
                await _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);
            }
            return true;
        }
    }
}
