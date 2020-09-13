using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Framework.Security.Captcha;
using Grand.Web.Areas.Admin.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class GeneralCommonSettingsModel : BaseGrandModel
    {
        public GeneralCommonSettingsModel()
        {
            StoreInformationSettings = new StoreInformationSettingsModel();
            SeoSettings = new SeoSettingsModel();
            SecuritySettings = new SecuritySettingsModel();
            PdfSettings = new PdfSettingsModel();
            LocalizationSettings = new LocalizationSettingsModel();
            FullTextSettings = new FullTextSettingsModel();
            GoogleAnalyticsSettings = new GoogleAnalyticsSettingsModel();
            DisplayMenuSettings = new DisplayMenuSettingsModel();
            KnowledgebaseSettings = new KnowledgebaseSettingsModel();
        }
        public string ActiveStoreScopeConfiguration { get; set; }
        public StoreInformationSettingsModel StoreInformationSettings { get; set; }
        public SeoSettingsModel SeoSettings { get; set; }
        public SecuritySettingsModel SecuritySettings { get; set; }
        public PdfSettingsModel PdfSettings { get; set; }
        public LocalizationSettingsModel LocalizationSettings { get; set; }
        public FullTextSettingsModel FullTextSettings { get; set; }
        public GoogleAnalyticsSettingsModel GoogleAnalyticsSettings { get; set; }
        public DisplayMenuSettingsModel DisplayMenuSettings { get; set; }
        public KnowledgebaseSettingsModel KnowledgebaseSettings { get; set; }

        #region Nested classes

        public partial class StoreInformationSettingsModel : BaseGrandModel
        {
            public StoreInformationSettingsModel()
            {
                this.AvailableStoreThemes = new List<ThemeConfigurationModel>();
            }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.StoreClosed")]
            public bool StoreClosed { get; set; }
            public bool StoreClosed_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultStoreTheme")]

            public string DefaultStoreTheme { get; set; }
            public bool DefaultStoreTheme_OverrideForStore { get; set; }
            public IList<ThemeConfigurationModel> AvailableStoreThemes { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.AllowCustomerToSelectTheme")]
            public bool AllowCustomerToSelectTheme { get; set; }
            public bool AllowCustomerToSelectTheme_OverrideForStore { get; set; }

            [UIHint("Picture")]
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.Logo")]
            public string LogoPictureId { get; set; }
            public bool LogoPictureId_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayEuCookieLawWarning")]
            public bool DisplayEuCookieLawWarning { get; set; }
            public bool DisplayEuCookieLawWarning_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayPrivacyPreference")]
            public bool DisplayPrivacyPreference { get; set; }
            public bool DisplayPrivacyPreference_OverrideForStore { get; set; }
            

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.FacebookLink")]
            public string FacebookLink { get; set; }
            public bool FacebookLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.TwitterLink")]
            public string TwitterLink { get; set; }
            public bool TwitterLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.YoutubeLink")]
            public string YoutubeLink { get; set; }
            public bool YoutubeLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.InstagramLink")]
            public string InstagramLink { get; set; }
            public bool InstagramLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.LinkedInLink")]
            public string LinkedInLink { get; set; }
            public bool LinkedInLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PinterestLink")]
            public string PinterestLink { get; set; }
            public bool PinterestLink_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.StoreInDatabaseContactUsForm")]
            public bool StoreInDatabaseContactUsForm { get; set; }
            public bool StoreInDatabaseContactUsForm_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SubjectFieldOnContactUsForm")]
            public bool SubjectFieldOnContactUsForm { get; set; }
            public bool SubjectFieldOnContactUsForm_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.UseSystemEmailForContactUsForm")]
            public bool UseSystemEmailForContactUsForm { get; set; }
            public bool UseSystemEmailForContactUsForm_OverrideForStore { get; set; }

            #region Nested classes

            public partial class ThemeConfigurationModel
            {
                public string ThemeName { get; set; }
                public string ThemeTitle { get; set; }
                public string PreviewImageUrl { get; set; }
                public string PreviewText { get; set; }
                public bool SupportRtl { get; set; }
                public bool Selected { get; set; }
            }

            #endregion
        }

        public partial class SeoSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PageTitleSeparator")]

            public string PageTitleSeparator { get; set; }
            public bool PageTitleSeparator_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PageTitleSeoAdjustment")]
            public int PageTitleSeoAdjustment { get; set; }
            public bool PageTitleSeoAdjustment_OverrideForStore { get; set; }
            public SelectList PageTitleSeoAdjustmentValues { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultTitle")]

            public string DefaultTitle { get; set; }
            public bool DefaultTitle_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultMetaKeywords")]

            public string DefaultMetaKeywords { get; set; }
            public bool DefaultMetaKeywords_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultMetaDescription")]

            public string DefaultMetaDescription { get; set; }
            public bool DefaultMetaDescription_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.GenerateProductMetaDescription")]

            public bool GenerateProductMetaDescription { get; set; }
            public bool GenerateProductMetaDescription_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ConvertNonWesternChars")]
            public bool ConvertNonWesternChars { get; set; }
            public bool ConvertNonWesternChars_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SeoCharConversion")]
            public string SeoCharConversion { get; set; }
            public bool SeoCharConversion_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CanonicalUrlsEnabled")]
            public bool CanonicalUrlsEnabled { get; set; }
            public bool CanonicalUrlsEnabled_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EnableJsBundling")]
            public bool EnableJsBundling { get; set; }
            public bool EnableJsBundling_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EnableCssBundling")]
            public bool EnableCssBundling { get; set; }
            public bool EnableCssBundling_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.TwitterMetaTags")]
            public bool TwitterMetaTags { get; set; }
            public bool TwitterMetaTags_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.OpenGraphMetaTags")]
            public bool OpenGraphMetaTags { get; set; }
            public bool OpenGraphMetaTags_OverrideForStore { get; set; }

            [UIHint("Picture")]
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.StorePicture")]
            public string StorePictureId { get; set; }
            public bool StorePictureId_OverrideForStore { get; set; }
        }

        public partial class SecuritySettingsModel : BaseGrandModel
        {
            public SecuritySettingsModel()
            {
                this.AvailableReCaptchaVersions = new List<SelectListItem>();
            }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EncryptionKey")]

            public string EncryptionKey { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.AdminAreaAllowedIpAddresses")]

            public string AdminAreaAllowedIpAddresses { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EnableXSRFProtectionForAdminArea")]
            public bool EnableXsrfProtectionForAdminArea { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EnableXSRFProtectionForPublicStore")]
            public bool EnableXsrfProtectionForPublicStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.HoneypotEnabled")]
            public bool HoneypotEnabled { get; set; }


            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaEnabled")]
            public bool CaptchaEnabled { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnLoginPage")]
            public bool CaptchaShowOnLoginPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnRegistrationPage")]
            public bool CaptchaShowOnRegistrationPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnPasswordRecoveryPage")]
            public bool CaptchaShowOnPasswordRecoveryPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnContactUsPage")]
            public bool CaptchaShowOnContactUsPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnEmailWishlistToFriendPage")]
            public bool CaptchaShowOnEmailWishlistToFriendPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnEmailProductToFriendPage")]
            public bool CaptchaShowOnEmailProductToFriendPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage")]
            public bool CaptchaShowOnAskQuestionPage { get; set; }


            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnBlogCommentPage")]
            public bool CaptchaShowOnBlogCommentPage { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnArticleCommentPage")]
            public bool CaptchaShowOnArticleCommentPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnNewsCommentPage")]
            public bool CaptchaShowOnNewsCommentPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnProductReviewPage")]
            public bool CaptchaShowOnProductReviewPage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnApplyVendorPage")]
            public bool CaptchaShowOnApplyVendorPage { get; set; }


            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.reCaptchaPublicKey")]

            public string ReCaptchaPublicKey { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.reCaptchaPrivateKey")]

            public string ReCaptchaPrivateKey { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.reCaptchaVersion")]
            public ReCaptchaVersion ReCaptchaVersion { get; set; }
            public IList<SelectListItem> AvailableReCaptchaVersions { get; set; }
        }

        public partial class PdfSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PdfLogo")]
            [UIHint("Picture")]
            public string LogoPictureId { get; set; }
            public bool LogoPictureId_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisablePdfInvoicesForPendingOrders")]
            public bool DisablePdfInvoicesForPendingOrders { get; set; }
            public bool DisablePdfInvoicesForPendingOrders_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.InvoiceHeaderText")]
            public string InvoiceHeaderText { get; set; }
            public bool InvoiceHeaderText_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.InvoiceFooterText")]
            public string InvoiceFooterText { get; set; }
            public bool InvoiceFooterText_OverrideForStore { get; set; }

        }

        public partial class LocalizationSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.UseImagesForLanguageSelection")]
            public bool UseImagesForLanguageSelection { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.AutomaticallyDetectLanguage")]
            public bool AutomaticallyDetectLanguage { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.LoadAllLocaleRecordsOnStartup")]
            public bool LoadAllLocaleRecordsOnStartup { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.LoadAllLocalizedPropertiesOnStartup")]
            public bool LoadAllLocalizedPropertiesOnStartup { get; set; }
            
        }

        public partial class FullTextSettingsModel : BaseGrandModel
        {
            public bool Supported { get; set; }

            public bool Enabled { get; set; }

        }

        public partial class GoogleAnalyticsSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.GoogleAnalyticsPrivateKey")]
            public string gaprivateKey { get; set; }
            public bool gaprivateKey_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.GoogleAnalyticsServiceAccountEmail")]
            public string gaserviceAccountEmail { get; set; }
            public bool gaserviceAccountEmail_OverrideForStore { get; set; }

            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.GoogleAnalyticsViewID")]
            public string gaviewID { get; set; }
            public bool gaviewID_OverrideForStore { get; set; }

        }

        public partial class DisplayMenuSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayHomePageMenu")]
            public bool DisplayHomePageMenu { get; set; }
            public bool DisplayHomePageMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayNewProductsMenu")]
            public bool DisplayNewProductsMenu { get; set; }
            public bool DisplayNewProductsMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplaySearchMenu")]
            public bool DisplaySearchMenu { get; set; }
            public bool DisplaySearchMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayCustomerMenu")]
            public bool DisplayCustomerMenu { get; set; }
            public bool DisplayCustomerMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayBlogMenu")]
            public bool DisplayBlogMenu { get; set; }
            public bool DisplayBlogMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayForumsMenu")]
            public bool DisplayForumsMenu { get; set; }
            public bool DisplayForumsMenu_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DisplayMenuSettings.DisplayContactUsMenu")]
            public bool DisplayContactUsMenu { get; set; }
            public bool DisplayContactUsMenu_OverrideForStore { get; set; }
        }

        public partial class KnowledgebaseSettingsModel : BaseGrandModel
        {
            [GrandResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.KnowledgebaseSettings.Enabled")]
            public bool Enabled { get; set; }
            public bool Enabled_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.Knowledgebase.AllowNotRegisteredUsersToLeaveComments")]
            public bool AllowNotRegisteredUsersToLeaveComments { get; set; }
            public bool AllowNotRegisteredUsersToLeaveComments_OverrideForStore { get; set; }
            [GrandResourceDisplayName("Admin.Configuration.Settings.Knowledgebase.NotifyAboutNewArticleComments")]
            public bool NotifyAboutNewArticleComments { get; set; }
            public bool NotifyAboutNewArticleComments_OverrideForStore { get; set; }
        }

        #endregion
    }
}