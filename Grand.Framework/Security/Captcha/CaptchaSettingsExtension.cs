using Grand.Services.Localization;

namespace Grand.Framework.Security.Captcha
{
    public static class CaptchaSettingsExtension
    {
        public static string GetWrongCaptchaMessage(this CaptchaSettings captchaSettings,
            ILocalizationService localizationService)
        {
            if (captchaSettings.ReCaptchaVersion == ReCaptchaVersion.Version2)
                return localizationService.GetResource("Common.WrongCaptchaV2");
            return string.Empty;
        }
    }
}