using Grand.Services.Customers;
using Grand.Web.Commands.Models.Customers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Customers
{
    public class CurrentPositionCommandHandler : IRequestHandler<CurrentPositionCommand, bool>
    {
        private readonly ICustomerCoordinatesService _customerCoordinateService;

        public CurrentPositionCommandHandler(ICustomerCoordinatesService customerCoordinateService)
        {
            _customerCoordinateService = customerCoordinateService;
        }

        public async Task<bool> Handle(CurrentPositionCommand request, CancellationToken cancellationToken)
        {
            if (request.Customer == null)
                throw new ArgumentNullException("Customer");

            if (request.Model == null)
                throw new ArgumentNullException("Model");

            await _customerCoordinateService.SaveGeoCoordinate(request.Customer, request.Model.Longitude, request.Model.Latitude);

            return true;
        }
    }
}
