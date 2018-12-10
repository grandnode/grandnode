using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("li", Attributes = ForAttributeName)]
    public class TabTagHelper: TagHelper
    {
        private const string ForAttributeName = "tab-index";

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName(ForAttributeName)]
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

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var selectedIndex = GetSelectedTabIndex();

            if (selectedIndex == CurrentIndex)
            {
                output.Attributes.SetAttribute("class", "k-state-active");
            }
        }
    }
}
