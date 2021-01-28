using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Data;
using Grand.Services.Notifications.Customers;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public class CustomerCoordinatesService : ICustomerCoordinatesService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;

        public CustomerCoordinatesService(
            IRepository<Customer> customerRepository,
            IWorkContext workContext,
            IMediator mediator)
        {
            _customerRepository = customerRepository;
            _workContext = workContext;
            _mediator = mediator;
        }

        public Task<(double longitude, double latitude)> GetGeoCoordinate()
        {
            return GetGeoCoordinate(_workContext.CurrentCustomer);
        }

        public async Task<(double longitude, double latitude)> GetGeoCoordinate(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            return await Task.FromResult((customer.Coordinates.X, customer.Coordinates.Y));
        }

        public async Task SaveGeoCoordinate(double longitude, double latitude)
        {
            await SaveGeoCoordinate(_workContext.CurrentCustomer, longitude, latitude);
        }

        public async Task SaveGeoCoordinate(Customer customer, double longitude, double latitude)
        {
            var coordinates = new MongoDB.Driver.GeoJsonObjectModel.GeoJson2DCoordinates(longitude, latitude);
            customer.Coordinates = coordinates;

            var builder = Builders<Customer>.Filter;
            var filter = builder.Eq(x => x.Id, customer.Id);
            var update = Builders<Customer>.Update
                .Set(x => x.Coordinates, coordinates);

            //update customer
            await _customerRepository.Collection.UpdateOneAsync(filter, update);

            //raise event       
            await _mediator.Publish(new CustomerCoordinatesEvent(customer));
        }
    }
}
