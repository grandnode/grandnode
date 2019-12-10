using System.Collections.Generic;

namespace Grand.Web.Models.Boards
{
    public partial class BoardsIndexModel
    {
        public BoardsIndexModel()
        {
            ForumGroups = new List<ForumGroupModel>();
        }
        
        public IList<ForumGroupModel> ForumGroups { get; set; }
    }
}