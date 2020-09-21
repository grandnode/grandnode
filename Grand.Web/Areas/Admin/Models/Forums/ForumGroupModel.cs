using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Forums
{
    public partial class ForumGroupModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}