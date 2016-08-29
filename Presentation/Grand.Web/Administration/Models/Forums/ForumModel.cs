﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Forums;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Forums
{
    [Validator(typeof(ForumValidator))]
    public partial class ForumModel : BaseNopEntityModel
    {
        public ForumModel()
        {
            ForumGroups = new List<ForumGroupModel>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId")]
        public string ForumGroupId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Forums.Forum.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public List<ForumGroupModel> ForumGroups { get; set; }
    }
}