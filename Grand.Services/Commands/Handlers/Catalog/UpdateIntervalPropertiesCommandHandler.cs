using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Data;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class UpdateIntervalPropertiesCommandHandler : IRequestHandler<UpdateIntervalPropertiesCommand, bool>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product ID
        /// </remarks>
        private const string PRODUCTS_BY_ID_KEY = "Grand.product.id-{0}";

        private readonly IRepository<Product> _productRepository;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;

        public UpdateIntervalPropertiesCommandHandler(IRepository<Product> productRepository, IMediator mediator, ICacheManager cacheManager)
        {
            _productRepository = productRepository;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }

        public async Task<bool> Handle(UpdateIntervalPropertiesCommand request, CancellationToken cancellationToken)
        {
            if (request.Product == null)
                throw new ArgumentNullException("product");

            var filter = Builders<Product>.Filter.Eq("Id", request.Product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.Interval, request.Interval)
                    .Set(x => x.IntervalUnitId, (int)request.IntervalUnit)
                    .Set(x => x.IncBothDate, request.IncludeBothDates);

            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(PRODUCTS_BY_ID_KEY, request.Product.Id));

            //event notification
            await _mediator.EntityUpdated(request.Product);

            return true;
        }
    }
}
