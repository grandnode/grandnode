using Grand.Core.Caching;
using Grand.Services.Media;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("source", ParentTag = "picture")]
    public class PictureSourceTagHelper : TagHelper
    {
        private const string PICTURE_PATH = "Grand.Picture.{0}.{1}";

        [HtmlAttributeName("picture-id")]
        public string PictureId { set; get; }

        [HtmlAttributeName("picture-size")]
        public int PictureSize { set; get; }

        private readonly IPictureService _pictureService;
        private readonly ICacheManager _cacheManager;

        public PictureSourceTagHelper(IPictureService pictureService, ICacheManager cacheManager)
        {
            _pictureService = pictureService;
            _cacheManager = cacheManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(PictureId))
            {
                var cacheKey = string.Format(PICTURE_PATH, PictureId, PictureSize);
                var pictureurl = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return await _pictureService.GetPictureUrl(PictureId, PictureSize, showDefaultPicture: false);
                });
                var srcset = new TagHelperAttribute("srcset", pictureurl);
                output.Attributes.Add(srcset);
            }
            base.Process(context, output);
        }
    }
}