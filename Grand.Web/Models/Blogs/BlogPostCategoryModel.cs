﻿using Grand.Core.Models;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPostCategoryModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }
        public int BlogPostCount { get; set; }
    }
}
