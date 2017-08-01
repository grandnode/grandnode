using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Models.Boards
{
    public partial class TopicMoveModel : BaseGrandEntityModel
    {
        public TopicMoveModel()
        {
            ForumList = new List<SelectListItem>();
        }

        public string ForumSelected { get; set; }
        public string TopicSeName { get; set; }

        public IEnumerable<SelectListItem> ForumList { get; set; }
    }
}