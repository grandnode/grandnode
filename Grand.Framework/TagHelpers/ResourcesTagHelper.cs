using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("resources", Attributes = AttributeType)]
    public class ResourcesTagHelper : TagHelper
    {
        private const string AttributeType = "asp-type";

        [HtmlAttributeName(AttributeType)]
        public ResourceType Type { get; set; }

        private readonly IResourceManager _resourceManager;

        public ResourcesTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            switch (Type)
            {
                case ResourceType.ScriptHeader:
                    _resourceManager.RenderHeadScript(output.Content);
                    break;

                case ResourceType.ScriptFooter:
                    _resourceManager.RenderFootScript(output.Content);
                    break;

                default:
                    break;
            }

            output.TagName = null;
        }
    }
}
