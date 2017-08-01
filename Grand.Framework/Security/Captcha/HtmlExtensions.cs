using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Core.Infrastructure;

namespace Grand.Framework.Security.Captcha
{
    public static class HtmlExtensions
    {
        public static IHtmlContent GenerateCaptcha(this IHtmlHelper helper)
        {
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();

            var captchaControl = new GRecaptchaControl(captchaSettings.ReCaptchaVersion)
            {
                Theme = captchaSettings.ReCaptchaTheme,
                Id = "recaptcha",
                PublicKey = captchaSettings.ReCaptchaPublicKey,
                Language = captchaSettings.ReCaptchaLanguage
            };
            var captchaControlHtml = captchaControl.RenderControl();
            return new HtmlString(captchaControlHtml);
        }
    }
}