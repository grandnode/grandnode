using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Auction service interface
    /// </summary>
    public partial interface IAuctionService
    {
        /// <summary>
        /// Deletes bid
        /// </summary>
        /// <param name="bid"></param>
        Task DeleteBid(Bid bid);

        /// <summary>
        /// Inserts bid
        /// </summary>
        /// <param name="bid"></param>
        Task InsertBid(Bid bid);

        /// <summary>
        /// Updates bid
        /// </summary>
        /// <param name="bid"></param>
        Task UpdateBid(Bid bid);

        /// <summary>
        /// Gets bids for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Bids</returns>
        Task<IPagedList<Bid>> GetBidsByProductId(string productId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets bids for Customer Id
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>Bids</returns>
        Task<IPagedList<Bid>> GetBidsByCustomerId(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets bid for specified Id
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns>Bid</returns>
        Task<Bid> GetBid(string Id);

        /// <summary>
        /// Get latest bid for product Id
        /// </summary>
        /// <param name="productId">productId</param>
        /// <returns>Bid</returns>
        Task<Bid> GetLatestBid(string productId);

        /// <summary>
        /// Updates highest bid
        /// </summary>
        /// <param name="product"></param>
        /// <param name="bid"></param>
        /// <param name="bidder"></param>
        Task UpdateHighestBid(Product product, decimal bid, string bidder);

        /// <summary>
        /// Updates auction ended
        /// </summary>
        /// <param name="product"></param>
        /// <param name="ended"></param>
        /// <param name="enddate"></param>
        Task UpdateAuctionEnded(Product product, bool ended, bool enddate = false);

        /// <summary>
        /// Gets auctions that have to be ended
        /// </summary>
        Task<IList<Product>> GetAuctionsToEnd();

        /// <summary>
        /// New bid
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="product"></param>
        /// <param name="store"></param>
        /// <param name="warehouseId"></param>
        /// <param name="language"></param>
        /// <param name="amount"></param>
        Task NewBid(Customer customer, Product product, Store store, Language language, string warehouseId, decimal amount);

        /// <summary>
        /// Cancel bid for product
        /// </summary>
        /// <param name="orderId">order id</param>
        Task CancelBidByOrder(string orderId);
    }
}