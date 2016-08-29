﻿using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Templates;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Templates
{
    [Validator(typeof(CategoryTemplateValidator))]
    public partial class CategoryTemplateModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.System.Templates.Category.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.System.Templates.Category.ViewPath")]
        [AllowHtml]
        public string ViewPath { get; set; }

        [NopResourceDisplayName("Admin.System.Templates.Category.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}