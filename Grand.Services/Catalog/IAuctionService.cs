using Grand.Core;
using Grand.Core.Domain.Catalog;
using System.Collections.Generic;

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
        void DeleteBid(Bid bid);

        /// <summary>
        /// Inserts bid
        /// </summary>
        /// <param name="bid"></param>
        void InsertBid(Bid bid);

        /// <summary>
        /// Updates bid
        /// </summary>
        /// <param name="bid"></param>
        void UpdateBid(Bid bid);

        /// <summary>
        /// Gets bids for product Id
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>Bids</returns>
        IPagedList<Bid> GetBidsByProductId(string productId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets bids for Customer Id
        /// </summary>
        /// <param name="customerId">Customer Id</param>
        /// <returns>Bids</returns>
        IPagedList<Bid> GetBidsByCustomerId(string customerId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets bid for specified Id
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns>Bid</returns>
        Bid GetBid(string Id);

        /// <summary>
        /// Get latest bid for product Id
        /// </summary>
        /// <param name="productId">productId</param>
        /// <returns>Bid</returns>
        Bid GetLatestBid(string productId);

        /// <summary>
        /// Updates highest bid
        /// </summary>
        /// <param name="product"></param>
        /// <param name="bid"></param>
        /// <param name="bidder"></param>
        void UpdateHighestBid(Product product, decimal bid, string bidder);

        /// <summary>
        /// Updates auction ended
        /// </summary>
        /// <param name="product"></param>
        /// <param name="ended"></param>
        void UpdateAuctionEnded(Product product, bool ended);

        /// <summary>
        /// Gets auctions that have to be ended
        /// </summary>
        IList<Product> GetAuctionsToEnd();
    }
}