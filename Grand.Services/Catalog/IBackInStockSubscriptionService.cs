using Grand.Domain;
using Grand.Domain.Catalog;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Back in stock subscription service interface
    /// </summary>
    public partial interface IBackInStockSubscriptionService
    {
        /// <summary>
        /// Delete a back in stock subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        Task DeleteSubscription(BackInStockSubscription subscription);

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="storeId">Store identifier; pass "" to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Subscriptions</returns>
        Task<IPagedList<BackInStockSubscription>> GetAllSubscriptionsByCustomerId(string customerId,
            string storeId = "", int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets all subscriptions
        /// </summary>
        /// <param name="customerId">Customer id</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="attrxml">Attribute xml</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <returns>Subscriptions</returns>
        Task<BackInStockSubscription> FindSubscription(string customerId, string productId, string attributexml, string storeId, string warehouseId);

        /// <summary>
        /// Gets a subscription
        /// </summary>
        /// <param name="subscriptionId">Subscription identifier</param>
        /// <returns>Subscription</returns>
        Task<BackInStockSubscription> GetSubscriptionById(string subscriptionId);

        /// <summary>
        /// Inserts subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        Task InsertSubscription(BackInStockSubscription subscription);

        /// <summary>
        /// Updates subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        Task UpdateSubscription(BackInStockSubscription subscription);

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="warehouse">Warehouse ident</param>
        /// <returns>Number of sent email</returns>
        Task SendNotificationsToSubscribers(Product product, string warehouse);

        /// <summary>
        /// Send notification to subscribers
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributeXml">Attribute xml</param>
        /// <param name="warehouse">Warehouse ident</param>
        /// <returns>Number of sent email</returns>
        Task SendNotificationsToSubscribers(Product product, string attributeXml, string warehouse);
    }
}
