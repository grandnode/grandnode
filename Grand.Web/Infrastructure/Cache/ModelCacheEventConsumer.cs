using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Polls;
using Grand.Core.Domain.Topics;
using Grand.Core.Domain.Vendors;
using Grand.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        
        //Topics
        INotificationHandler<EntityInserted<Topic>>,
        INotificationHandler<EntityUpdated<Topic>>,
        INotificationHandler<EntityDeleted<Topic>>,
        //Orders
        INotificationHandler<EntityInserted<Order>>,
        INotificationHandler<EntityUpdated<Order>>,
        INotificationHandler<EntityDeleted<Order>>,
        //Product picture mapping
        INotificationHandler<EntityInserted<ProductPicture>>,
        INotificationHandler<EntityUpdated<ProductPicture>>,
        INotificationHandler<EntityDeleted<ProductPicture>>,
        //polls
        INotificationHandler<EntityInserted<Poll>>,
        INotificationHandler<EntityUpdated<Poll>>,
        INotificationHandler<EntityDeleted<Poll>>,
        //blog posts
        INotificationHandler<EntityInserted<BlogPost>>,
        INotificationHandler<EntityUpdated<BlogPost>>,
        INotificationHandler<EntityDeleted<BlogPost>>,
        //blog post category
        INotificationHandler<EntityInserted<BlogCategory>>,
        INotificationHandler<EntityUpdated<BlogCategory>>,
        INotificationHandler<EntityDeleted<BlogCategory>>,
        //news items
        INotificationHandler<EntityInserted<NewsItem>>,
        INotificationHandler<EntityUpdated<NewsItem>>,
        INotificationHandler<EntityDeleted<NewsItem>>,
        //return requests
        INotificationHandler<EntityInserted<ReturnRequestAction>>,
        INotificationHandler<EntityUpdated<ReturnRequestAction>>,
        INotificationHandler<EntityDeleted<ReturnRequestAction>>,
        INotificationHandler<EntityInserted<ReturnRequestReason>>,
        INotificationHandler<EntityUpdated<ReturnRequestReason>>,
        INotificationHandler<EntityDeleted<ReturnRequestReason>>,
        //templates
        INotificationHandler<EntityInserted<CategoryTemplate>>,
        INotificationHandler<EntityUpdated<CategoryTemplate>>,
        INotificationHandler<EntityDeleted<CategoryTemplate>>,
        INotificationHandler<EntityInserted<ManufacturerTemplate>>,
        INotificationHandler<EntityUpdated<ManufacturerTemplate>>,
        INotificationHandler<EntityDeleted<ManufacturerTemplate>>,
        INotificationHandler<EntityInserted<ProductTemplate>>,
        INotificationHandler<EntityUpdated<ProductTemplate>>,
        INotificationHandler<EntityDeleted<ProductTemplate>>,
        INotificationHandler<EntityInserted<TopicTemplate>>,
        INotificationHandler<EntityUpdated<TopicTemplate>>,
        INotificationHandler<EntityDeleted<TopicTemplate>>,
        //checkout attributes
        INotificationHandler<EntityInserted<CheckoutAttribute>>,
        INotificationHandler<EntityUpdated<CheckoutAttribute>>,
        INotificationHandler<EntityDeleted<CheckoutAttribute>>,
        //shopping cart items
        INotificationHandler<EntityUpdated<ShoppingCartItem>>
    {


        private readonly ICacheManager _cacheManager;

        public ModelCacheEventConsumer(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }


        //Topics
        public async Task Handle(EntityInserted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Topic> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.SITEMAP_PATTERN_KEY);
        }

        //Orders
        public async Task Handle(EntityInserted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Order> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }

        //Product picture mappings
        public async Task Handle(EntityInserted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityUpdated<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }
        public async Task Handle(EntityDeleted<ProductPicture> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_PATTERN_KEY, eventMessage.Entity.ProductId));
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }

        //Polls
        public async Task Handle(EntityInserted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<Poll> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.POLLS_PATTERN_KEY);
        }

        //Blog posts
        public async Task Handle(EntityInserted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogPost> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }

        //Blog post category
        public async Task Handle(EntityInserted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<BlogCategory> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.BLOG_PATTERN_KEY);
        }

        //News items
        public async Task Handle(EntityInserted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<NewsItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.NEWS_PATTERN_KEY);
        }

        //retunr requests
        public async Task Handle(EntityInserted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestAction> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTACTIONS_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ReturnRequestReason> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.RETURNREQUESTREASONS_PATTERN_KEY);
        }

        //templates
        public async Task Handle(EntityInserted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ManufacturerTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<ProductTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityInserted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<TopicTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.TOPIC_TEMPLATE_PATTERN_KEY);
        }

        //checkout attributes
        public async Task Handle(EntityInserted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CheckoutAttribute> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(ModelCacheEventConst.CHECKOUTATTRIBUTES_PATTERN_KEY);
        }

        //shopping cart items        
        public async Task Handle(EntityUpdated<ShoppingCartItem> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheManager.RemoveByPrefix(string.Format(ModelCacheEventConst.CART_PICTURE_PATTERN_KEY, eventMessage.Entity.ProductId));
        }

    }
}
