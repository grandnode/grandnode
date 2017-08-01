using System.Collections.Generic;

namespace Grand.Web.Models.Boards
{
    public partial  class ForumGroupModel
    {
        public ForumGroupModel()
        {
            this.Forums = new List<ForumRowModel>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string SeName { get; set; }

        public IList<ForumRowModel> Forums { get; set; }
    }
}