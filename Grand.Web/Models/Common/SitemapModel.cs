using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Topics;
using System.Collections.Generic;

namespace Grand.Web.Models.Common
{
    public partial class SitemapModel : BaseGrandModel
    {
        public SitemapModel()
        {
            Products = new List<ProductOverviewModel>();
            Categories = new List<CategoryModel>();
            Manufacturers = new List<ManufacturerModel>();
            Topics = new List<TopicModel>();
        }
        public IList<ProductOverviewModel> Products { get; set; }
        public IList<CategoryModel> Categories { get; set; }
        public IList<ManufacturerModel> Manufacturers { get; set; }
        public IList<TopicModel> Topics { get; set; }

        public bool NewsEnabled { get; set; }
        public bool BlogEnabled { get; set; }
        public bool ForumEnabled { get; set; }
        public bool KnowledgebaseEnabled { get; set; }
    }
}