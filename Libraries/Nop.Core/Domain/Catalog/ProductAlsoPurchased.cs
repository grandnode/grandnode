
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product also purchased
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ProductAlsoPurchased : BaseEntity
    {
        private ICollection<Purchase> _purchased;
        /// <summary>
        /// Gets or sets product id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the collection of Purchased
        /// </summary>
        public virtual ICollection<Purchase> Purchased
        {
            get { return _purchased ?? (_purchased = new List<Purchase>()); }
            protected set { _purchased = value; }
        }

       
    }

    [BsonIgnoreExtraElements]
    public class Purchase : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the create date order 
        /// </summary>
        public DateTime CreatedOrderOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the quantity product 
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

    }
}
