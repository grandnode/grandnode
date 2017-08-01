using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Blogs
{
    public partial class BlogPostYearModel : BaseGrandModel
    {
        public BlogPostYearModel()
        {
            Months = new List<BlogPostMonthModel>();
        }
        public int Year { get; set; }
        public IList<BlogPostMonthModel> Months { get; set; }
    }
    public partial class BlogPostMonthModel : BaseGrandModel
    {
        public int Month { get; set; }

        public int BlogPostCount { get; set; }
    }
}