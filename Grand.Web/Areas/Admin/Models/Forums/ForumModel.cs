using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Forums;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Forums
{
    [Validator(typeof(ForumValidator))]
    public partial class ForumModel : BaseGrandEntityModel
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