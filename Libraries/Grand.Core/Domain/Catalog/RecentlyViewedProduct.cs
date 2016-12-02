using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Core.Domain.Catalog
{
    public partial class RecentlyViewedProduct: BaseEntity
    {
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public DateTime CreatedOnUtc { get; set; }

    }
}
