using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Grand.Domain.Localization;

namespace Grand.Domain.Orders
{
    public partial class OrderTag : BaseEntity
    {
        private ICollection<OrderIds> _orders;

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

        /// <summary>
        /// Gets or sets the order's tags
        /// </summary>
        public virtual ICollection<OrderIds> Orders
        {
            get { return _orders ?? (_orders = new List<OrderIds>()); }
            protected set { _orders = value; }

        }
    }        
    
    public class OrderIds
    {
        public string OrderId { get; set; }
    }

    public class OrderTagComparer : IEqualityComparer<OrderTag>
    {
        public bool Equals(OrderTag x, OrderTag y)
        {
            return x.Name == y.Name;
        }


        public int GetHashCode(OrderTag obj)
        {
            return 0;
        }
    }
}
