using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers.Admin
{
    [HtmlTargetElement("items", ParentTag = "admin-tabstrip")]
    public partial class AdminTabStripItemsTagHelper : TagHelper
    {
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "ul";
            return Task.CompletedTask;
        }
    }
}
