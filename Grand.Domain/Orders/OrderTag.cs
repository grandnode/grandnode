using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Grand.Domain.Localization;

namespace Grand.Domain.Orders
{
    public partial class OrderTag : BaseEntity
    {
        private ICollection<string> _orders;

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
        public virtual ICollection<string> Orders {
            get { return _orders ?? (_orders = new List<string>()); }
            protected set { _orders = value; }

        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            OrderTag tag = (OrderTag)obj;
            return ( Name == tag.Name);
        }
    }
}
