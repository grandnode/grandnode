using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Media;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class CategoryModel : BaseGrandEntityModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            SubCategories = new List<SubCategoryModel>();
            CategoryBreadcrumb = new List<CategoryModel>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public string Flag { get; set; }
        public string FlagStyle { get; set; }
        public string Icon { get; set; }
        public PictureModel PictureModel { get; set; }
        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }        
        public IList<SubCategoryModel> SubCategories { get; set; }
        public IList<ProductOverviewModel> FeaturedProducts { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }        
		#region Nested Classes
        public partial class SubCategoryModel : BaseGrandEntityModel
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
            }
            public string Name { get; set; }
            public string SeName { get; set; }
            public string Description { get; set; }
            public string Flag { get; set; }
            public string FlagStyle { get; set; }
            public PictureModel PictureModel { get; set; }
        }

		#endregion
    }
}