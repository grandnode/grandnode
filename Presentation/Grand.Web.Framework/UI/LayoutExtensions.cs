﻿using System.Web.Mvc;
using Grand.Core.Infrastructure;

namespace Grand.Web.Framework.UI
{
    public static class LayoutExtensions
    {

        /// <summary>
        /// Add title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Title part</param>
        public static void AddTitleParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder  = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddTitleParts(part);
        }
        /// <summary>
        /// Append title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Title part</param>
        public static void AppendTitleParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder  = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendTitleParts(part);
        }
        /// <summary>
        /// Generate all title parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="addDefaultTitle">A value indicating whether to insert a default title</param>
        /// <param name="part">Title part</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopTitle(this HtmlHelper html, bool addDefaultTitle, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendTitleParts(part);
            return MvcHtmlString.Create(html.Encode(pageHeadBuilder.GenerateTitle(addDefaultTitle)));
        }


        /// <summary>
        /// Add meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        public static void AddMetaDescriptionParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaDescriptionParts(part);
        }
        /// <summary>
        /// Append meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        public static void AppendMetaDescriptionParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaDescriptionParts(part);
        }
        /// <summary>
        /// Generate all description parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopMetaDescription(this HtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendMetaDescriptionParts(part);
            return MvcHtmlString.Create(html.Encode(pageHeadBuilder.GenerateMetaDescription()));
        }


        /// <summary>
        /// Add meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        public static void AddMetaKeywordParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaKeywordParts(part);
        }
        /// <summary>
        /// Append meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        public static void AppendMetaKeywordParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaKeywordParts(part);
        }
        /// <summary>
        /// Generate all keyword parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopMetaKeywords(this HtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendMetaKeywordParts(part);
            return MvcHtmlString.Create(html.Encode(pageHeadBuilder.GenerateMetaKeywords()));
        }


        /// <summary>
        /// Add script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Script part</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AddScriptParts(this HtmlHelper html, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            AddScriptParts(html, ResourceLocation.Head, part, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Add script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="part">Script part</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AddScriptParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddScriptParts(location, part, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Append script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Script part</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AppendScriptParts(this HtmlHelper html, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            AppendScriptParts(html, ResourceLocation.Head, part, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Append script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="part">Script part</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AppendScriptParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendScriptParts(location, part, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Generate all script parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="urlHelper">URL Helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="bundleFiles">A value indicating whether to bundle script elements</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopScripts(this HtmlHelper html, UrlHelper urlHelper, 
            ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return MvcHtmlString.Create(pageHeadBuilder.GenerateScripts(urlHelper, location, bundleFiles));
        }



        /// <summary>
        /// Add CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">CSS part</param>
        public static void AddCssFileParts(this HtmlHelper html, string part, bool excludeFromBundle = false)
        {
            AddCssFileParts(html, ResourceLocation.Head, part, excludeFromBundle);
        }
        /// <summary>
        /// Add CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="part">CSS part</param>
        public static void AddCssFileParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddCssFileParts(location, part, excludeFromBundle);
        }
        /// <summary>
        /// Append CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">CSS part</param>
        public static void AppendCssFileParts(this HtmlHelper html, string part, bool excludeFromBundle = false)
        {
            AppendCssFileParts(html, ResourceLocation.Head, part, excludeFromBundle);
        }
        /// <summary>
        /// Append CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="part">CSS part</param>
        public static void AppendCssFileParts(this HtmlHelper html, ResourceLocation location, string part, bool excludeFromBundle = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendCssFileParts(location, part, excludeFromBundle);
        }
        /// <summary>
        /// Generate all CSS parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="urlHelper">URL Helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="bundleFiles">A value indicating whether to bundle script elements</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopCssFiles(this HtmlHelper html, UrlHelper urlHelper,
            ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return MvcHtmlString.Create(pageHeadBuilder.GenerateCssFiles(urlHelper, location, bundleFiles));
        }


        /// <summary>
        /// Add canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AddCanonicalUrlParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddCanonicalUrlParts(part);
        }
        /// <summary>
        /// Append canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AppendCanonicalUrlParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendCanonicalUrlParts(part);
        }
        /// <summary>
        /// Generate all canonical URL parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopCanonicalUrls(this HtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendCanonicalUrlParts(part);
            return MvcHtmlString.Create(pageHeadBuilder.GenerateCanonicalUrls());
        }

        /// <summary>
        /// Add any custom element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AddHeadCustomParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddHeadCustomParts(part);
        }
        /// <summary>
        /// Append any custom element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AppendHeadCustomParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendHeadCustomParts(part);
        }
        /// <summary>
        /// Generate all custom elements
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <returns>Generated string</returns>
        public static MvcHtmlString NopHeadCustom(this HtmlHelper html)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return MvcHtmlString.Create(pageHeadBuilder.GenerateHeadCustom());
        }
    }
}
