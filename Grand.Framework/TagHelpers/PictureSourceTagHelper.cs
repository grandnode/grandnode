using Grand.Services.Media;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("source", ParentTag = "picture")]
    public class PictureSourceTagHelper : TagHelper
    {
        [HtmlAttributeName("picture-id")]
        public string PictureId { set; get; }

        [HtmlAttributeName("picture-size")]
        public int PictureSize { set; get; }

        private readonly IPictureService _pictureService;

        public PictureSourceTagHelper(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(PictureId))
            {
                var pictureurl = await _pictureService.GetPictureUrl(PictureId, PictureSize, showDefaultPicture: false);
                var srcset = new TagHelperAttribute("srcset", pictureurl);
                output.Attributes.Add(srcset);
            }
            await base.ProcessAsync(context, output);
        }
    }
}