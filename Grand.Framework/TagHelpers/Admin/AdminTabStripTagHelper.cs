using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("admin-tabstrip")]
    public partial class AdminTabStripTagHelper : TagHelper
    {
        [HtmlAttributeName("SetTabPos")]
        public bool SetTabPos { get; set; } = false;

        [HtmlAttributeName("Name")]
        public string Name { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData[typeof(AdminTabContentTagHelper).FullName] = new List<string>();

            var content = await output.GetChildContentAsync();
            var list = (List<string>)ViewContext.ViewData[typeof(AdminTabContentTagHelper).FullName];

            output.TagName = "div";
            output.Attributes.SetAttribute("id", Name);
            output.Attributes.SetAttribute("style", "display:none");

            var sb = new StringBuilder();
            sb.AppendLine("<script>");
            sb.AppendLine("$(document).ready(function () {");
            if (SetTabPos)
            {
                sb.AppendLine(" var tabPos = 'left'; ");
                sb.AppendLine(" if (window.devicePixelRatio == 2) {");
                sb.AppendLine("   tabPos = 'top'; }");
            }
            sb.AppendLine($" $('#{Name}').kendoTabStrip({{ ");
            if (SetTabPos)
                sb.AppendLine("   tabPosition: tabPos,");

            sb.AppendLine("   select: tabstrip_on_tab_select");
            sb.AppendLine("  });");
            sb.AppendLine($"$('#{Name}').show();");

            sb.AppendLine("})");

            sb.AppendLine("</script>");
            output.PostContent.AppendHtml(string.Concat(list));
            output.PostElement.AppendHtml(sb.ToString());
        }

    }
}
