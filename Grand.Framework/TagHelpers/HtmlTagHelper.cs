using Grand.Core;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("html", Attributes = ForAttributeName)]
    public class HtmlTagHelper : TagHelper
    {
        private const string ForAttributeName = "use-lang";

        [HtmlAttributeName(ForAttributeName)]
        public bool UseLanguage { set; get; }

        private readonly IWorkContext _workContext;

        public HtmlTagHelper(IWorkContext workContext)
        {
            _workContext = workContext;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (UseLanguage)
            {
                output.Attributes.Add("lang", _workContext.WorkingLanguage.UniqueSeoCode);
                if(_workContext.WorkingLanguage.Rtl)
                    output.Attributes.Add("dir", "rtl");
            }
            return Task.CompletedTask;
        }
    }
}