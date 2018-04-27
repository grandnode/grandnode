using Grand.Core;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{

    [HtmlTargetElement("admin-select", Attributes = ForAttributeName)]
    public class AdminSelectTagHelper : Microsoft.AspNetCore.Mvc.TagHelpers.SelectTagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisplayHintAttributeName = "asp-display-hint";

        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        public AdminSelectTagHelper(IHtmlGenerator generator, IWorkContext workContext, ILocalizationService localizationService) : base(generator)
        {
            _workContext = workContext;
            _localizationService = localizationService;
        }

        [HtmlAttributeName(DisplayHintAttributeName)]
        public bool DisplayHint { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            output.TagName = "select";
            output.TagMode = TagMode.StartTagAndEndTag;
            var classValue = output.Attributes.ContainsName("class")
                                ? $"{output.Attributes["class"].Value}"
                                : "form-control k-input";
            output.Attributes.SetAttribute("class", classValue);
            
        }
    }

    
}