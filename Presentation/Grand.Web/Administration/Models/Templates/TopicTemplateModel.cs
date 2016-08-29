﻿using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Templates;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Templates
{
    [Validator(typeof(TopicTemplateValidator))]
    public partial class TopicTemplateModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.System.Templates.Topic.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.System.Templates.Topic.ViewPath")]
        [AllowHtml]
        public string ViewPath { get; set; }

        [NopResourceDisplayName("Admin.System.Templates.Topic.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}