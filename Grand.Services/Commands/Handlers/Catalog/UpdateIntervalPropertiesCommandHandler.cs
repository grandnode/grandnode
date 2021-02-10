using Grand.Core.Caching;
using Grand.Core.Caching.Constants;
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
        private readonly IRepository<Product> _productRepository;
        private readonly IMediator _mediator;
        private readonly ICacheBase _cacheBase;

        public UpdateIntervalPropertiesCommandHandler(IRepository<Product> productRepository, IMediator mediator, ICacheBase cacheManager)
        {
            _productRepository = productRepository;
            _mediator = mediator;
            _cacheBase = cacheManager;
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
            await _cacheBase.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, request.Product.Id));

            //event notification
            await _mediator.EntityUpdated(request.Product);

            return true;
        }
    }
}
