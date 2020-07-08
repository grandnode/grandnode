namespace Grand.Domain.Orders
{
    /// <summary>
    /// Represents a shoping cart type
    /// </summary>
    public enum ShoppingCartType
    {
        /// <summary>
        /// Shopping cart
        /// </summary>
        ShoppingCart = 1,
        /// <summary>
        /// Wishlist
        /// </summary>
        Wishlist = 2,
        /// <summary>
        /// Auctions
        /// </summary>
        Auctions = 3,
        /// <summary>
        /// On hold cart
        /// </summary>
        OnHoldCart = 10
    }
}
