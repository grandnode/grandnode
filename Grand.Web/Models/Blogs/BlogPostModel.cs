using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPostModel : BaseGrandEntityModel
    {
        public BlogPostModel()
        {
            Tags = new List<string>();
            Comments = new List<BlogCommentModel>();
            AddNewComment = new AddBlogCommentModel();
            PictureModel = new PictureModel();
        }

        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public string Title { get; set; }
        public PictureModel PictureModel { get; set; }
        public string Body { get; set; }
        public string BodyOverview { get; set; }
        public bool AllowComments { get; set; }
        public int NumberOfComments { get; set; }
        public DateTime CreatedOn { get; set; }

        public IList<string> Tags { get; set; }

        public IList<BlogCommentModel> Comments { get; set; }
        public AddBlogCommentModel AddNewComment { get; set; }
    }
}