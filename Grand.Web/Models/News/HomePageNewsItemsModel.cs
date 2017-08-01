using System;
using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.News
{
    public partial class HomePageNewsItemsModel : BaseGrandModel, ICloneable
    {
        public HomePageNewsItemsModel()
        {
            NewsItems = new List<NewsItemModel>();
        }

        public string WorkingLanguageId { get; set; }
        public IList<NewsItemModel> NewsItems { get; set; }

        public object Clone()
        {
            //we use a shallow copy (deep clone is not required here)
            return this.MemberwiseClone();
        }
    }
}