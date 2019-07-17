using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("tabstrip-item")]
    public partial class AdminTabStripItemTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("Text")]
        public string Text { get; set; }

        [HtmlAttributeName("tab-index")]
        public int CurrentIndex { set; get; }

        private int GetSelectedTabIndex()
        {
            int index = 0;
            string dataKey = "Grand.selected-tab-index";
            if (this.ViewContext.ViewData[dataKey] is int)
            {
                index = (int)this.ViewContext.ViewData[dataKey];
            }
            if (this.ViewContext.TempData[dataKey] is int)
            {
                index = (int)this.ViewContext.TempData[dataKey];
            }

            if (index < 0)
                index = 0;

            return index;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await output.GetChildContentAsync();
            output.TagName = "li";

            var selectedIndex = GetSelectedTabIndex();
            if (selectedIndex == CurrentIndex)
            {
                output.Attributes.SetAttribute("class", "k-state-active");
            }

            output.Content.AppendHtml(Text);
        }

    }
}
