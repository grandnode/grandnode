using Grand.Framework.UI;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("meta-keywords", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("meta-keywords", Attributes = PartAttributeName)]
    public class MetaKeywordsTagHelper : TagHelper
    {
        private const string PartAttributeName = "asp-part";
        [HtmlAttributeName(PartAttributeName)]
        public string Attribute { get; set; }

        private readonly IPageHeadBuilder _pageHeadBuilder;

        public MetaKeywordsTagHelper(IPageHeadBuilder pageHeadBuilder)
        {
            _pageHeadBuilder = pageHeadBuilder;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Attribute))
            {
                _pageHeadBuilder.AppendMetaKeywordParts(Attribute);
            }
            output.TagName = "meta";
            output.Attributes.Add("name", "keywords");
            output.Attributes.Add("content", _pageHeadBuilder.GenerateMetaKeywords());
            return Task.CompletedTask;
        }

    }
}
