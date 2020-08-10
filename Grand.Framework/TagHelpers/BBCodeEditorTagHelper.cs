using Grand.Core;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("bbc-code", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("bbc-code", Attributes = PartAttributeName)]
    public class BBCodeEditorTagHelper : TagHelper
    {
        private const string PartAttributeName = "asp-name";
        [HtmlAttributeName(PartAttributeName)]
        public string Attribute { get; set; }

        private readonly IWebHelper _webHelper;

        public BBCodeEditorTagHelper(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            var sb = new StringBuilder();

            var storeLocation = _webHelper.GetStoreLocation();
            string bbEditorWebRoot = String.Format("{0}content/", storeLocation);

            sb.AppendFormat("<script src=\"{0}content/bbeditor/ed.js\" ></script>", storeLocation);
            sb.Append(Environment.NewLine);
            sb.Append("<script language=\"javascript\" type=\"text/javascript\">");
            sb.Append(Environment.NewLine);
            sb.AppendFormat("edToolbar('{0}','{1}');", Attribute, bbEditorWebRoot);
            sb.Append(Environment.NewLine);
            sb.Append("</script>");
            sb.Append(Environment.NewLine);

            var content = new HtmlString(sb.ToString());
            output.Content.SetHtmlContent(content);

            return Task.CompletedTask;
        }

    }
}
