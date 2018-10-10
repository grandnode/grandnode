using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace Grand.Framework.Security.Captcha
{
    public class GRecaptchaControl
    {
        private const string RECAPTCHA_API_URL_VERSION2 = "https://www.google.com/recaptcha/api.js?onload=onloadCallback&render=explicit";

        public string Id { get; set; }
        public string Theme { get; set; }
        public string PublicKey { get; set; }
        public string Language { get; set; }

        private readonly ReCaptchaVersion _version;

        public GRecaptchaControl(ReCaptchaVersion version = ReCaptchaVersion.Version2)
        {
            _version = version;
        }

        public string RenderControl()
        {
            SetTheme();

            if (_version == ReCaptchaVersion.Version2)
            {
                var scriptCallbackTag = new TagBuilder("script");
                scriptCallbackTag.TagRenderMode = TagRenderMode.Normal;
                scriptCallbackTag.InnerHtml.AppendHtml(string.Format("var onloadCallback = function() {{grecaptcha.render('{0}', {{'sitekey' : '{1}', 'theme' : '{2}' }});}};", Id, PublicKey, Theme));
               
                var captchaTag = new TagBuilder("div");
                captchaTag.TagRenderMode = TagRenderMode.Normal;
                captchaTag.Attributes.Add("id", Id);
               
                var scriptLoadApiTag = new TagBuilder("script");
                scriptLoadApiTag.TagRenderMode = TagRenderMode.Normal;
                scriptLoadApiTag.Attributes.Add("src", RECAPTCHA_API_URL_VERSION2 + (string.IsNullOrEmpty(Language) ? "" : string.Format("&hl={0}", Language)));
                scriptLoadApiTag.Attributes.Add("async", null);
                scriptLoadApiTag.Attributes.Add("defer", null);

                return scriptCallbackTag.RenderHtmlContent() + captchaTag.RenderHtmlContent() + scriptLoadApiTag.RenderHtmlContent();
            }

            throw new NotSupportedException("Specified version is not supported");
        }

        private void SetTheme()
        {
            var themes = new[] {"white", "blackglass", "red", "clean", "light", "dark"};

            if (_version == ReCaptchaVersion.Version2)
            {
                switch (Theme.ToLower())
                {
                    case "clean":
                    case "red":
                    case "white":
                        Theme = "light";
                        break;
                    case "blackglass":
                        Theme = "dark";
                        break;
                    default:
                        if (!themes.Contains(Theme.ToLower()))
                        {
                            Theme = "light";
                        }
                        break;
                }
            }
        }
    }
}