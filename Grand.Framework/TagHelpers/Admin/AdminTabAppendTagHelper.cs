using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("admin-tab-append", Attributes = "tab-strip-name, tab-name, tab-content")]
    public partial class AdminTabAppendTagHelper : TagHelper
    {
        private const string TabStripName = "tab-strip-name";
        private const string TabName = "tab-name";
        private const string TabContent = "tab-content";

        [HtmlAttributeName(TabName)]
        public string Name { get; set; }

        [HtmlAttributeName(TabContent)]
        public string Content { get; set; }

        [HtmlAttributeName(TabStripName)]
        public string StripName { get; set; }


        public AdminTabAppendTagHelper()
        {
        }

        public override void Process(TagHelperContext tagHelperContext, TagHelperOutput output)
        {
            output.SuppressOutput();

            var builder = new StringBuilder();
            builder.Append("<script>");
            builder.Append(" $(document).ready(function() {");
            builder.Append($"   $('#{StripName}').kendoTabStrip().data('kendoTabStrip').append({{");
            builder.Append($"     text: '{Name}',");
            builder.Append($"     content: '{Content}'");
            builder.Append("   });");
            builder.Append(" });");
            builder.Append("</script>");
            output.Content.AppendHtml(builder.ToString());
        }
    }
}
