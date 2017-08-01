using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("input", Attributes = ForAttributeName)]
    public class GrandInputTagHelper : InputTagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName("asp-disabled")]
        public string IsDisabled { set; get; }

        public GrandInputTagHelper(IHtmlGenerator generator) : base(generator)
        {
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            base.Process(context, output);
        }
    }
}