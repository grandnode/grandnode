﻿namespace Grand.Framework.UI
{
    /// <summary>
    /// Page head builder
    /// </summary>
    public partial interface IPageHeadBuilder
    {
        void AddTitleParts(string part);
        void AppendTitleParts(string part);
        string GenerateTitle(bool addDefaultTitle);

        void AddMetaDescriptionParts(string part);
        void AppendMetaDescriptionParts(string part);
        string GenerateMetaDescription();

        void AddMetaKeywordParts(string part);
        void AppendMetaKeywordParts(string part);
        string GenerateMetaKeywords();

        void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAync);
        void AppendScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync);
        string GenerateScripts(ResourceLocation location, bool? bundleFiles = null);

        void AddCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false);
        void AppendCssFileParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle = false);
        string GenerateCssFiles(ResourceLocation location, bool? bundleFiles = null);

        void AddCanonicalUrlParts(string part);
        void AppendCanonicalUrlParts(string part);
        string GenerateCanonicalUrls();

        void AddHeadCustomParts(string part);
        void AppendHeadCustomParts(string part);
        string GenerateHeadCustom();

        void AddPageCssClassParts(string part);
        void AppendPageCssClassParts(string part);
        string GeneratePageCssClasses();

        /// <summary>
        /// Specify "edit page" URL
        /// </summary>
        /// <param name="url">URL</param>
        void AddEditPageUrl(string url);
        /// <summary>
        /// Get "edit page" URL
        /// </summary>
        /// <returns>URL</returns>
        string GetEditPageUrl();

    }
}
