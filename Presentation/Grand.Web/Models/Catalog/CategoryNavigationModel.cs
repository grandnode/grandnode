using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

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
    }
}