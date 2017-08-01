using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;

using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Forums;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Forums
{
    [Validator(typeof(ForumGroupValidator))]
    public partial class ForumGroupModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}