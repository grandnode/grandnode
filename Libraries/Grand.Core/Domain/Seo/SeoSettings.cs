﻿using System.Collections.Generic;
using Grand.Core.Configuration;

namespace Grand.Core.Domain.Seo
{
    /// <summary>
    /// SEO settings
    /// </summary>
    public class SeoSettings : ISettings
    {
        /// <summary>
        /// Page title separator
        /// </summary>
        public string PageTitleSeparator { get; set; }
        /// <summary>
        /// Page itle SEO adjustment
        /// </summary>
        public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }
        /// <summary>
        /// Default title
        /// </summary>
        public string DefaultTitle { get; set; }
        /// <summary>
        /// Default META keywords
        /// </summary>
        public string DefaultMetaKeywords { get; set; }
        /// <summary>
        /// Default META description
        /// </summary>
        public string DefaultMetaDescription { get; set; }
        /// <summary>
        /// A value indicating whether product META descriptions will be generated automatically (if not entered)
        /// </summary>
        public bool GenerateProductMetaDescription { get; set; }
        /// <summary>
        /// A value indicating whether we should conver non-wetern chars to western ones
        /// </summary>
        public bool ConvertNonWesternChars { get; set; }
        /// <summary>
        /// A value indicating whether unicode chars are allowed
        /// </summary>
        public bool AllowUnicodeCharsInUrls { get; set; }
        /// <summary>
        /// A value indicating whether canonical URL tags should be used
        /// </summary>
        public bool CanonicalUrlsEnabled { get; set; }
        /// <summary>
        /// WWW requires (with or without WWW)
        /// </summary>
        public WwwRequirement WwwRequirement { get; set; }
        /// <summary>
        /// A value indicating whether JS file bundling and minification is enabled
        /// </summary>
        public bool EnableJsBundling { get; set; }
        /// <summary>
        /// A value indicating whether CSS file bundling and minification is enabled
        /// </summary>
        public bool EnableCssBundling { get; set; }
        /// <summary>
        /// A value indicating whether Twitter META tags should be generated
        /// </summary>
        public bool TwitterMetaTags { get; set; }
        /// <summary>
        /// A value indicating whether Open Graph META tags should be generated
        /// </summary>
        public bool OpenGraphMetaTags { get; set; }
        /// <summary>
        /// Slugs (sename) reserved for some other needs
        /// </summary>
        public List<string> ReservedUrlRecordSlugs { get; set; }
    }
}