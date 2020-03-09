﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Framework.TagHelpers.Admin
{
    internal static class DefaultDisplayTemplates
    {
        public static async Task<IHtmlContent> BooleanTemplate(IHtmlHelper htmlHelper)
        {
            bool? value = null;
            if (htmlHelper.ViewData.Model != null)
            {
                value = Convert.ToBoolean(htmlHelper.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return htmlHelper.ViewData.ModelMetadata.IsNullableValueType ?
                await BooleanTemplateDropDownList(value) :
                await BooleanTemplateCheckbox(value ?? false);
        }

        private static async Task<IHtmlContent> BooleanTemplateCheckbox(bool value)
        {
            var inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value)
            {
                inputTag.Attributes["checked"] = "checked";
            }

            inputTag.TagRenderMode = TagRenderMode.SelfClosing;
            return await Task.FromResult(inputTag);
        }

        private static async Task<IHtmlContent> BooleanTemplateDropDownList(bool? value)
        {
            var selectTag = new TagBuilder("select");
            selectTag.AddCssClass("list-box");
            selectTag.AddCssClass("tri-state");
            selectTag.Attributes["disabled"] = "disabled";

            foreach (var item in TriStateValues(value))
            {
                selectTag.InnerHtml.AppendHtml(DefaultHtmlGenerator.GenerateOption(item, item.Text));
            }

            return await Task.FromResult(selectTag);
        }

        // Will soon need to be shared with the default editor templates implementations.
        internal static List<SelectListItem> TriStateValues(bool? value)
        {
            return new List<SelectListItem>
            {
                new SelectListItem("Not Set", string.Empty, !value.HasValue),
                new SelectListItem("True", "true", (value == true)),
                new SelectListItem("False", "false", (value == false)),
            };
        }

        public static async Task<IHtmlContent> CollectionTemplate(IHtmlHelper htmlHelper)
        {
            var model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                return HtmlString.Empty;
            }

            var enumerable = model as IEnumerable;
            if (enumerable == null)
            {
                // Only way we could reach here is if user passed templateName: "Collection" to a Display() overload.
                throw new InvalidOperationException($"Collection {model.GetType().FullName}");
            }

            var elementMetadata = htmlHelper.ViewData.ModelMetadata.ElementMetadata;
            Debug.Assert(elementMetadata != null);
            var typeInCollectionIsNullableValueType = elementMetadata.IsNullableValueType;

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();

            // Use typeof(string) instead of typeof(object) for IEnumerable collections. Neither type is Nullable<T>.
            if (elementMetadata.ModelType == typeof(object))
            {
                elementMetadata = metadataProvider.GetMetadataForType(typeof(string));
            }

            var oldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;
            try
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

                var collection = model as ICollection;
                var result = collection == null ? new HtmlContentBuilder() : new HtmlContentBuilder(collection.Count);
                var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();
                var viewBufferScope = serviceProvider.GetRequiredService<IViewBufferScope>();

                var index = 0;
                foreach (var item in enumerable)
                {
                    var itemMetadata = elementMetadata;
                    if (item != null && !typeInCollectionIsNullableValueType)
                    {
                        itemMetadata = metadataProvider.GetMetadataForType(item.GetType());
                    }

                    var modelExplorer = new ModelExplorer(
                        metadataProvider,
                        container: htmlHelper.ViewData.ModelExplorer,
                        metadata: itemMetadata,
                        model: item);
                    var fieldName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", oldPrefix, index++);

                    var templateBuilder = new TemplateBuilder(
                        viewEngine,
                        viewBufferScope,
                        htmlHelper.ViewContext,
                        htmlHelper.ViewData,
                        modelExplorer,
                        htmlFieldName: fieldName,
                        templateName: null,
                        readOnly: true,
                        additionalViewData: null);
                    result.AppendHtml(await templateBuilder.Build());
                }

                return result;
            }
            finally
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public static async Task<IHtmlContent> DecimalTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == htmlHelper.ViewData.Model)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", htmlHelper.ViewData.Model);
            }
            return await StringTemplate(htmlHelper);
        }

        public static async Task<IHtmlContent> EmailAddressTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = "mailto:" + ((htmlHelper.ViewData.Model == null) ?
                string.Empty :
                htmlHelper.ViewData.Model.ToString());
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return await Task.FromResult(HyperlinkTemplate(uriString, linkedText));
        }

        public static async Task<IHtmlContent> HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.ModelMetadata.HideSurroundingHtml)
            {
                return HtmlString.Empty;
            }
            return await StringTemplate(htmlHelper);
        }

        public static async Task<IHtmlContent> HtmlTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(new HtmlString(htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString()));
        }

        public static async Task<IHtmlContent> ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;

            if (modelExplorer.Model == null)
            {
                return new HtmlString(modelExplorer.Metadata.NullDisplayText);
            }

            if (templateInfo.TemplateDepth > 1)
            {
                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    text = htmlHelper.Encode(text);
                }

                return new HtmlString(text);
            }

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();
            var viewBufferScope = serviceProvider.GetRequiredService<IViewBufferScope>();

            var content = new HtmlContentBuilder(modelExplorer.Metadata.Properties.Count);
            foreach (var propertyExplorer in modelExplorer.Properties)
            {
                var propertyMetadata = propertyExplorer.Metadata;
                if (!ShouldShow(propertyExplorer, templateInfo))
                {
                    continue;
                }

                var templateBuilder = new TemplateBuilder(
                    viewEngine,
                    viewBufferScope,
                    htmlHelper.ViewContext,
                    htmlHelper.ViewData,
                    propertyExplorer,
                    htmlFieldName: propertyMetadata.PropertyName,
                    templateName: null,
                    readOnly: true,
                    additionalViewData: null);

                var templateBuilderResult = await templateBuilder.Build();
                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = propertyMetadata.GetDisplayName();
                    if (!string.IsNullOrEmpty(label))
                    {
                        var labelTag = new TagBuilder("div");
                        labelTag.InnerHtml.SetContent(label);
                        labelTag.AddCssClass("display-label");
                        content.AppendLine(labelTag);
                    }

                    var valueDivTag = new TagBuilder("div");
                    valueDivTag.AddCssClass("display-field");
                    valueDivTag.InnerHtml.SetHtmlContent(templateBuilderResult);
                    content.AppendLine(valueDivTag);
                }
                else
                {
                    content.AppendHtml(templateBuilderResult);
                }
            }

            return content;
        }

        private static bool ShouldShow(ModelExplorer modelExplorer, TemplateInfo templateInfo)
        {
            return
                modelExplorer.Metadata.ShowForDisplay &&
                !modelExplorer.Metadata.IsComplexType &&
                !templateInfo.Visited(modelExplorer);
        }

        public static async Task<IHtmlContent> StringTemplate(IHtmlHelper htmlHelper)
        {
            var value = htmlHelper.ViewData.TemplateInfo.FormattedModelValue;
            if (value == null)
            {
                return HtmlString.Empty;
            }
            return await Task.FromResult(new StringHtmlContent(value.ToString()));
        }

        public static async Task<IHtmlContent> UrlTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = (htmlHelper.ViewData.Model == null) ? string.Empty : htmlHelper.ViewData.Model.ToString();
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return await Task.FromResult(HyperlinkTemplate(uriString, linkedText));
        }

        // Neither uriString nor linkedText need be encoded prior to calling this method.
        private static IHtmlContent HyperlinkTemplate(string uriString, string linkedText)
        {
            var hyperlinkTag = new TagBuilder("a");
            hyperlinkTag.MergeAttribute("href", uriString);
            hyperlinkTag.InnerHtml.SetContent(linkedText);
            return hyperlinkTag;
        }
    }
}
