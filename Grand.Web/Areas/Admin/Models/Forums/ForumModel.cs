using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Forums
{
    public partial class ForumModel : BaseEntityModel
    {
        public ForumModel()
        {
            ForumGroups = new List<ForumGroupModel>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId")]
        public string ForumGroupId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public List<ForumGroupModel> ForumGroups { get; set; }
    }
}