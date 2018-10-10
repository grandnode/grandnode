using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class CategoryNavigationModel : BaseGrandModel
    {
        public CategoryNavigationModel()
        {
            Categories = new List<CategorySimpleModel>();
        }

        public string CurrentCategoryId { get; set; }
        public List<CategorySimpleModel> Categories { get; set; }

        public class CategoryLineModel : BaseGrandModel
        {
            public string CurrentCategoryId { get; set; }
            public CategorySimpleModel Category { get; set; }
        }
    }
}