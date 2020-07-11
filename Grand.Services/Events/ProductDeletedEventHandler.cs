using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Seo;
using Grand.Core.Events;
using Grand.Services.Catalog;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class ProductDeletedEventHandler : INotificationHandler<EntityDeleted<Product>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IProductTagService _productTagService;

        public ProductDeletedEventHandler(
            IRepository<Product> productRepository,
            IRepository<CustomerRoleProduct> customerRoleProductRepository,
            IRepository<Customer> customerRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductReview> productReviewRepository,
            IProductTagService productTagService)
        {
            _productRepository = productRepository;
            _customerRoleProductRepository = customerRoleProductRepository;
            _customerRepository = customerRepository;
            _urlRecordRepository = urlRecordRepository;
            _productTagRepository = productTagRepository;
            _productReviewRepository = productReviewRepository;
            _productTagService = productTagService;
        }

        public async Task Handle(EntityDeleted<Product> notification, CancellationToken cancellationToken)
        {
            //delete related product
            var builderRelated = Builders<Product>.Update;
            var updatefilterRelated = builderRelated.PullFilter(x => x.RelatedProducts, y => y.ProductId2 == notification.Entity.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilterRelated);

            //delete similar product
            var builderSimilar = Builders<Product>.Update;
            var updatefilterSimilar = builderSimilar.PullFilter(x => x.SimilarProducts, y => y.ProductId2 == notification.Entity.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilterSimilar);

            //delete cross sales product
            var builderCross = Builders<Product>.Update;
            var updatefilterCross = builderCross.Pull(x => x.CrossSellProduct, notification.Entity.Id);
            await _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilterCross);


            //delete review
            var filtersProductReview = Builders<ProductReview>.Filter;
            var filterProdReview = filtersProductReview.Eq(x => x.ProductId, notification.Entity.Id);
            await _productReviewRepository.Collection.DeleteManyAsync(filterProdReview);

            //delete from shopping cart
            var builder = Builders<Customer>.Update;
            var updatefilter = builder.PullFilter(x => x.ShoppingCartItems, y => y.ProductId == notification.Entity.Id);
            await _customerRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter);

            //delete customer role product
            var filtersCrp = Builders<CustomerRoleProduct>.Filter;
            var filterCrp = filtersCrp.Eq(x => x.ProductId, notification.Entity.Id);
            await _customerRoleProductRepository.Collection.DeleteManyAsync(filterCrp);

            //delete url
            var filters = Builders<UrlRecord>.Filter;
            var filter = filters.Eq(x => x.EntityId, notification.Entity.Id);
            filter = filter & filters.Eq(x => x.EntityName, "Product");
            await _urlRecordRepository.Collection.DeleteManyAsync(filter);

            //delete product tags
            var existingProductTags = _productTagRepository.Table.Where(x => notification.Entity.ProductTags.ToList().Contains(x.Name)).ToList();
            foreach (var tag in existingProductTags)
            {
                tag.ProductId = notification.Entity.Id;
                await _productTagService.DetachProductTag(tag);
            }

        }
    }
}
