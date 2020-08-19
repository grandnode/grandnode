using Grand.Framework.UI;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("title", Attributes = PartAttributeName)]
    [HtmlTargetElement("title", Attributes = DefaultAttributeName)]
    public class TitleTagHelper : TagHelper
    {
        private const string PartAttributeName = "asp-part";
        private const string DefaultAttributeName = "asp-default-title";

        [HtmlAttributeName(PartAttributeName)]
        public string Attribute { get; set; }

        [HtmlAttributeName(DefaultAttributeName)]
        public bool DefaultTitle { get; set; } = true;

        private readonly IPageHeadBuilder _pageHeadBuilder;

        public TitleTagHelper(IPageHeadBuilder pageHeadBuilder)
        {
            _pageHeadBuilder = pageHeadBuilder;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Attribute))
            {
                _pageHeadBuilder.AddTitleParts(Attribute);
            }
            var content = _pageHeadBuilder.GenerateTitle(DefaultTitle);
            output.Content.Append(content);
            return Task.CompletedTask;
        }

    }
}
