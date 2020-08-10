using Grand.Framework.UI;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("script-files", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("script-files", Attributes = AttributeNameLocation)]
    [HtmlTargetElement("script-files", Attributes = AttributeNameBundleFiles)]
    public class ScriptFilesTagHelper : TagHelper
    {
        private const string AttributeNameLocation = "asp-location";
        [HtmlAttributeName(AttributeNameLocation)]
        public ResourceLocation Location { get; set; }

        private const string AttributeNameBundleFiles = "asp-files";
        public bool? BundleFiles { get; set; } = null;

        private readonly IPageHeadBuilder _pageHeadBuilder;

        public ScriptFilesTagHelper(IPageHeadBuilder pageHeadBuilder)
        {
            _pageHeadBuilder = pageHeadBuilder;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.SetHtmlContent(_pageHeadBuilder.GenerateScripts(Location, BundleFiles));
            output.TagName = null;
            return Task.CompletedTask;
        }
    }
}