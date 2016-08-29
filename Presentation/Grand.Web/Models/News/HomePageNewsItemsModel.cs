using System;
using System.Collections.Generic;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.News
{
    public partial class HomePageNewsItemsModel : BaseNopModel, ICloneable
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