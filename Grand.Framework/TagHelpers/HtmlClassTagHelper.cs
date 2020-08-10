using Grand.Framework.UI;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("html-class", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("html-class", Attributes = ForAttributeName)]
    public class HtmlClassTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-name";

        [HtmlAttributeName(ForAttributeName)]
        public string Part { set; get; }

        private readonly IPageHeadBuilder _pageHeadBuilder;

        public HtmlClassTagHelper(IPageHeadBuilder pageHeadBuilder)
        {
            _pageHeadBuilder = pageHeadBuilder;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();
            if (!string.IsNullOrEmpty(Part))
            {
                _pageHeadBuilder.AddPageCssClassParts(Part);
            }
            return Task.CompletedTask;
        }
    }
}