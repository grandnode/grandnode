using Grand.Core.Models;
using Grand.Web.Models.Media;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.News
{
    public partial class NewsItemListModel : BaseModel
    {
        public NewsItemListModel()
        {
            PagingFilteringContext = new NewsPagingFilteringModel();
            NewsItems = new List<NewsItemModel>();
        }

        public string WorkingLanguageId { get; set; }
        public NewsPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<NewsItemModel> NewsItems { get; set; }

        public class NewsItemModel : BaseModel
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