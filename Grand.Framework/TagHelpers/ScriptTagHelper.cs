using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Grand.Framework.TagHelpers
{
    
    [HtmlTargetElement("script", Attributes = LocationAttributeName)]
    public class ScriptTagHelper : TagHelper
    {

        private const string LocationAttributeName = "asp-location";

        [HtmlAttributeName(LocationAttributeName)]
        public ScriptLocation Location { get; set; }

        private readonly IResourceManager _resourceManager;

        public ScriptTagHelper(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            var childContent = await output.GetChildContentAsync();

            var builder = new TagBuilder("script");
            builder.InnerHtml.AppendHtml(childContent);
            builder.TagRenderMode = TagRenderMode.Normal;

            foreach (var attribute in output.Attributes)
            {
                builder.Attributes.Add(attribute.Name, attribute.Value.ToString());
            }

            switch (Location)
            {
                case ScriptLocation.Header:
                    _resourceManager.RegisterHeadScript(builder);
                    break;

                case ScriptLocation.Footer:
                    _resourceManager.RegisterFootScript(builder);
                    break;

                default:
                    break;
            }
            
        }
        
    }
}
