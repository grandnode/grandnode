using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class CategoryNavigationModel : BaseModel
    {
        public CategoryNavigationModel()
        {
            Categories = new List<CategorySimpleModel>();
        }

        public string CurrentCategoryId { get; set; }
        public List<CategorySimpleModel> Categories { get; set; }

        public class CategoryLineModel : BaseModel
        {
            public string CurrentCategoryId { get; set; }
            public CategorySimpleModel Category { get; set; }
        }
    }
}