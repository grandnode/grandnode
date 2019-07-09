using Grand.Framework.Security.Captcha;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("captcha", TagStructure = TagStructure.WithoutEndTag)]
    public class GenerateCaptchaTagHelper : TagHelper
    {
        private readonly IHtmlHelper _htmlHelper;
        private readonly CaptchaSettings _captchaSettings;

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public GenerateCaptchaTagHelper(IHtmlHelper htmlHelper, CaptchaSettings captchaSettings)
        {
            _htmlHelper = htmlHelper;
            _captchaSettings = captchaSettings;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //generate captcha control
            var captchaControl = new GRecaptchaControl(_captchaSettings.ReCaptchaVersion)
            {
                Theme = _captchaSettings.ReCaptchaTheme,
                Id = "g-recaptcha-response-value-" + Guid.NewGuid().ToString("N"),
                PublicKey = _captchaSettings.ReCaptchaPublicKey,
                Language = _captchaSettings.ReCaptchaLanguage
            };
            var captchaControlHtml = captchaControl.RenderControl();

            //tag details
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Content.SetHtmlContent(captchaControlHtml);
        }
    }
}