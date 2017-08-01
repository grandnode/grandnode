using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class TopMenuModel : BaseGrandModel
    {
        public TopMenuModel()
        {
            Categories = new List<CategorySimpleModel>();
            Topics = new List<TopMenuTopicModel>();
            Manufacturers = new List<TopMenuManufacturerModel> ();
        }

        public IList<CategorySimpleModel> Categories { get; set; }
        public IList<TopMenuTopicModel> Topics { get; set; }
        public IList<TopMenuManufacturerModel> Manufacturers { get; set; }

        public bool BlogEnabled { get; set; }
        public bool NewProductsEnabled { get; set; }
        public bool ForumEnabled { get; set; }

        public bool DisplayHomePageMenu { get; set; }
        public bool DisplayNewProductsMenu { get; set; }
        public bool DisplaySearchMenu { get; set; }
        public bool DisplayCustomerMenu { get; set; }
        public bool DisplayBlogMenu { get; set; }
        public bool DisplayForumsMenu { get; set; }
        public bool DisplayContactUsMenu { get; set; }

        #region Nested classes

        public class TopMenuTopicModel : BaseGrandEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
        }

        public class TopMenuManufacturerModel : BaseGrandEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
        }


        public class CategoryLineModel : BaseGrandModel
        {
            public int Level { get; set; }
            public bool ResponsiveMobileMenu { get; set; }
            public CategorySimpleModel Category { get; set; }
        }

        #endregion
    }
}