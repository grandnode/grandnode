using Grand.Framework.UI;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("canonical-urls", TagStructure = TagStructure.WithoutEndTag)]
    public class CanonicalUrlsTagHelper : TagHelper
    {

        private readonly IPageHeadBuilder _pageHeadBuilder;

        public CanonicalUrlsTagHelper(IPageHeadBuilder pageHeadBuilder)
        {
            _pageHeadBuilder = pageHeadBuilder;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();
            output.Content.SetHtmlContent(_pageHeadBuilder.GenerateCanonicalUrls());
            return Task.CompletedTask;
        }
    }
}