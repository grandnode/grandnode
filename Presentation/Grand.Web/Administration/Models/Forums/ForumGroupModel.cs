using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Forums;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Forums
{
    [Validator(typeof(ForumGroupValidator))]
    public partial class ForumGroupModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}