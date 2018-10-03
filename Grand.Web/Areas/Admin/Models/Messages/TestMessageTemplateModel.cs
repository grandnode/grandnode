using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Messages;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    [Validator(typeof(TestMessageTemplateValidator))]
    public partial class TestMessageTemplateModel : BaseGrandEntityModel
    {
        public TestMessageTemplateModel()
        {
            Tokens = new List<string>();
        }

        public string LanguageId { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.MessageTemplates.Test.Tokens")]
        public List<string> Tokens { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.MessageTemplates.Test.SendTo")]
        public string SendTo { get; set; }
    }
}