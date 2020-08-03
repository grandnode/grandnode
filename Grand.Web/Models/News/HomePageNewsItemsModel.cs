using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.News
{
    public partial class HomePageNewsItemsModel : BaseGrandModel
    {
        public HomePageNewsItemsModel()
        {
            NewsItems = new List<NewsItemModel>();
        }

        public IList<NewsItemModel> NewsItems { get; set; }

        public class NewsItemModel : BaseGrandModel
        {
            public NewsItemModel()
            {
                PictureModel = new PictureModel();
            }

            public string Id { get; set; }
            public string SeName { get; set; }
            public string Title { get; set; }
            public PictureModel PictureModel { get; set; }
            public string Short { get; set; }
            public string Full { get; set; }
            public DateTime CreatedOn { get; set; }
        }
    }
}