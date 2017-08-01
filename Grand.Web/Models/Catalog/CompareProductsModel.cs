using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class CompareProductsModel : BaseGrandEntityModel
    {
        public CompareProductsModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public IList<ProductOverviewModel> Products { get; set; }

        public bool IncludeShortDescriptionInCompareProducts { get; set; }
        public bool IncludeFullDescriptionInCompareProducts { get; set; }
    }
}