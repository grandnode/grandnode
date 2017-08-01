using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Templates;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Templates
{
    [Validator(typeof(TopicTemplateValidator))]
    public partial class TopicTemplateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Topic.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.ViewPath")]
        
        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}