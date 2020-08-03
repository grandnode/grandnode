using Grand.Framework.Localization;
using Grand.Framework.Mapping;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Blogs
{
    public partial class BlogCategoryModel : BaseGrandEntityModel, ILocalizedModel<BlogCategoryLocalizedModel>, IStoreMappingModel
    {
        public BlogCategoryModel()
        {
            this.AvailableStores = new List<StoreModel>();
            Locales = new List<BlogCategoryLocalizedModel>();
        }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.Fields.SeName")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<BlogCategoryLocalizedModel> Locales { get; set; }
        //Store mapping
        public bool LimitedToStores { get; set; }
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }
    }
    public partial class BlogCategoryLocalizedModel : ILocalizedModelLocal
    {
        public string LanguageId { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.Fields.Name")]
        public string Name { get; set; }
    }

    public partial class AddBlogPostCategoryModel : BaseGrandModel
    {
        public AddBlogPostCategoryModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.SearchBlogTitle")]

        public string SearchBlogTitle { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.Blog.BlogCategory.SearchStore")]
        public string SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public string CategoryId { get; set; }

        public string[] SelectedBlogPostIds { get; set; }
    }
}
