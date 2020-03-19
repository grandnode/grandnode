using Grand.Core;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{

    [HtmlTargetElement("admin-label", Attributes = ForAttributeName)]
    public class LabelRequiredTagHelper : LabelTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisplayHintAttributeName = "asp-display-hint";

        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        public LabelRequiredTagHelper(IHtmlGenerator generator, IWorkContext workContext, ILocalizationService localizationService) : base(generator)
        {
            _workContext = workContext;
            _localizationService = localizationService;
        }

        [HtmlAttributeName(DisplayHintAttributeName)]
        public bool DisplayHint { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            output.TagName = "label";
            output.TagMode = TagMode.StartTagAndEndTag;
            var classValue = output.Attributes.ContainsName("class")
                                ? $"{output.Attributes["class"].Value}"
                                : "control-label col-md-3 col-sm-3";
            output.Attributes.SetAttribute("class", classValue);
            
            if (For.Metadata.AdditionalValues.TryGetValue("GrandResourceDisplayNameAttribute", out object value))
            {
                var resourceDisplayName = value as GrandResourceDisplayNameAttribute;
                var langId = _workContext.WorkingLanguage.Id;

                var resource = _localizationService.GetResource(
                    resourceDisplayName.ResourceKey.ToLowerInvariant(), langId, returnEmptyIfNotFound: true,
                    logIfNotFound: false);

                if (!string.IsNullOrEmpty(resource))
                {
                    output.Content.SetContent(resource);
                }

                if (resourceDisplayName != null && DisplayHint)
                {

                    var hintResource = _localizationService.GetResource(
                        resourceDisplayName.ResourceKey + ".Hint", langId, returnEmptyIfNotFound: true,
                        logIfNotFound: false);

                    if (!string.IsNullOrEmpty(hintResource))
                    {
                        TagBuilder i = new TagBuilder("i");
                        i.AddCssClass("help icon-question");
                        i.Attributes.Add("title", hintResource);
                        i.Attributes.Add("data-toggle", "tooltip");
                        i.Attributes.Add("data-placement", "top");
                        i.Attributes.Add("data-container", "body");
                        output.Content.AppendHtml(i.ToHtmlString());
                    }
                }

            }
        }
    }

    
}