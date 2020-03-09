﻿using Grand.Core.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Framework.UI
{
    /// <summary>
    /// Layout extensions
    /// </summary>
    public static class LayoutExtensions
    {
        /// <summary>
        /// Add title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Title part</param>
        public static void AddTitleParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddTitleParts(part);
        }
        /// <summary>
        /// Append title element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Title part</param>
        public static void AppendTitleParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendTitleParts(part);
        }
        /// <summary>
        /// Generate all title parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="addDefaultTitle">A value indicating whether to insert a default title</param>
        /// <param name="part">Title part</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandTitle(this IHtmlHelper html, bool addDefaultTitle = true, string part = "")
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            html.AppendTitleParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateTitle(addDefaultTitle)));
        }


        /// <summary>
        /// Add meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        public static void AddMetaDescriptionParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaDescriptionParts(part);
        }
        /// <summary>
        /// Append meta description element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        public static void AppendMetaDescriptionParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaDescriptionParts(part);
        }
        /// <summary>
        /// Generate all description parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta description part</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandMetaDescription(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            html.AppendMetaDescriptionParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateMetaDescription()));
        }


        /// <summary>
        /// Add meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        public static void AddMetaKeywordParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaKeywordParts(part);
        }
        /// <summary>
        /// Append meta keyword element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        public static void AppendMetaKeywordParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaKeywordParts(part);
        }
        /// <summary>
        /// Generate all keyword parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Meta keyword part</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandMetaKeywords(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            html.AppendMetaKeywordParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateMetaKeywords()));
        }


        /// <summary>
        /// Add script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public static void AddScriptParts(this IHtmlHelper html, string src, string debugSrc = "",
            bool excludeFromBundle = false, bool isAsync = false)
        {
            AddScriptParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Add script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public static void AddScriptParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Append script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public static void AppendScriptParts(this IHtmlHelper html, string src, string debugSrc = "",
            bool excludeFromBundle = false, bool isAsync = false)
        {
            AppendScriptParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Append script element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        /// <param name="isAsync">A value indicating whether to add an attribute "async" or not for js files</param>
        public static void AppendScriptParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        /// <summary>
        /// Generate all script parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="urlHelper">URL Helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="bundleFiles">A value indicating whether to bundle script elements</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateScripts(urlHelper, location, bundleFiles));
        }


        /// <summary>
        /// Add CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AddCssFileParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            AddCssFileParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle);
        }
        /// <summary>
        /// Add CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AddCssFileParts(this IHtmlHelper html, ResourceLocation location, 
            string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddCssFileParts(location, src, debugSrc, excludeFromBundle);
        }
        /// <summary>
        /// Append CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AppendCssFileParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            AppendCssFileParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle);
        }
        /// <summary>
        /// Append CSS element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="src">Script path (minified version)</param>
        /// <param name="debugSrc">Script path (full debug version). If empty, then minified version will be used</param>
        /// <param name="excludeFromBundle">A value indicating whether to exclude this script from bundling</param>
        public static void AppendCssFileParts(this IHtmlHelper html, ResourceLocation location, 
            string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendCssFileParts(location, src, debugSrc, excludeFromBundle);
        }
        /// <summary>
        /// Generate all CSS parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="urlHelper">URL Helper</param>
        /// <param name="location">A location of the script element</param>
        /// <param name="bundleFiles">A value indicating whether to bundle script elements</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandCssFiles(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateCssFiles(urlHelper, location, bundleFiles));
        }

        /// <summary>
        /// Add canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AddCanonicalUrlParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddCanonicalUrlParts(part);
        }
        /// <summary>
        /// Append canonical URL element to the <![CDATA[<head>]]>
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        public static void AppendCanonicalUrlParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendCanonicalUrlParts(part);
        }
        /// <summary>
        /// Generate all canonical URL parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">Canonical URL part</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandCanonicalUrls(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            html.AppendCanonicalUrlParts(part);
            return new HtmlString(pageHeadBuilder.GenerateCanonicalUrls());
        }


        /// <summary>
        /// Add any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AddHeadCustomParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddHeadCustomParts(part);
        }
        /// <summary>
        /// Append any custom element to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">The entire element. For example, <![CDATA[<meta name="msvalidate.01" content="123121231231313123123" />]]></param>
        public static void AppendHeadCustomParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendHeadCustomParts(part);
        }
        /// <summary>
        /// Generate all custom elements
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandHeadCustom(this IHtmlHelper html)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateHeadCustom());
        }


        /// <summary>
        /// Add CSS class to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">CSS class</param>
        public static void AddPageCssClassParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AddPageCssClassParts(part);
        }
        /// <summary>
        /// Append CSS class to the <![CDATA[<head>]]> element
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">CSS class</param>
        public static void AppendPageCssClassParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.AppendPageCssClassParts(part);
        }
        /// <summary>
        /// Generate all title parts
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="part">CSS class</param>
        /// <param name="includeClassElement">A value indicating whether to include "class" attributes</param>
        /// <returns>Generated string</returns>
        public static IHtmlContent GrandPageCssClasses(this IHtmlHelper html, string part = "", bool includeClassElement = true)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            html.AppendPageCssClassParts(part);
            var classes = pageHeadBuilder.GeneratePageCssClasses();

            if (string.IsNullOrEmpty(classes))
                return null;

            var result = includeClassElement ? string.Format("class=\"{0}\"", classes) : classes;
            return new HtmlString(result);
        }


        /// <summary>
        /// Specify system name of admin menu item that should be selected (expanded)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="systemName">System name</param>
        public static void SetActiveMenuItemSystemName(this IHtmlHelper html, string systemName)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            pageHeadBuilder.SetActiveMenuItemSystemName(systemName);
        }
        /// <summary>
        /// Get system name of admin menu item that should be selected (expanded)
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <returns>System name</returns>
        public static string GetActiveMenuItemSystemName(this IHtmlHelper html)
        {
            var pageHeadBuilder = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageHeadBuilder>();
            return pageHeadBuilder.GetActiveMenuItemSystemName();
        }
    }
}