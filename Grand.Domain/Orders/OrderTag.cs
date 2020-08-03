using System;
using System.Collections.Generic;
using System.Text;
using Grand.Domain.Localization;

namespace Grand.Domain.Orders
{
    public partial class OrderTag : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets order id
        /// </summary>
        public string OrderId { get; set; }
        
    }
}
