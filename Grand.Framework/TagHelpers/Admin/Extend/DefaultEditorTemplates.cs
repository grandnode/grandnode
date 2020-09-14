﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
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
    internal static class DefaultEditorTemplates
    {
        private const string HtmlAttributeKey = "htmlAttributes";
        private const string UsePasswordValue = "Switch.Microsoft.AspNetCore.Mvc.UsePasswordValue";

        public static async Task<IHtmlContent> BooleanTemplate(IHtmlHelper htmlHelper)
        {
            bool? value = null;
            if (htmlHelper.ViewData.Model != null)
            {
                value = Convert.ToBoolean(htmlHelper.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return htmlHelper.ViewData.ModelMetadata.IsNullableValueType ?
                await BooleanTemplateDropDownList(htmlHelper, value) :
                await BooleanTemplateCheckbox(htmlHelper, value ?? false);
        }

        private static async Task<IHtmlContent> BooleanTemplateCheckbox(IHtmlHelper htmlHelper, bool value)
        {
            return await Task.FromResult(htmlHelper.CheckBox(
                expression: null,
                isChecked: value,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "check-box")));
        }

        private static async Task<IHtmlContent> BooleanTemplateDropDownList(IHtmlHelper htmlHelper, bool? value)
        {
            return await Task.FromResult(htmlHelper.DropDownList(
                expression: null,
                selectList: DefaultDisplayTemplates.TriStateValues(value),
                optionLabel: null,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "list-box tri-state")));
        }

        public static async Task<IHtmlContent> CollectionTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var model = viewData.Model;
            if (model == null)
            {
                return HtmlString.Empty;
            }

            var enumerable = model as IEnumerable;
            if (enumerable == null)
            {
                // Only way we could reach here is if user passed templateName: "Collection" to an Editor() overload.
                throw new InvalidOperationException($"Collection, {model.GetType().FullName}");
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

            var oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            try
            {
                viewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

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
                        readOnly: false,
                        additionalViewData: null);
                    result.AppendHtml(await templateBuilder.Build());
                }

                return result;
            }
            finally
            {
                viewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
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

        public static async Task<IHtmlContent> HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var model = viewData.Model;

            IHtmlContent display;
            if (viewData.ModelMetadata.HideSurroundingHtml)
            {
                display = null;
            }
            else
            {
                display = await DefaultDisplayTemplates.StringTemplate(htmlHelper);
            }

            var htmlAttributesObject = viewData[HtmlAttributeKey];
            var hidden = htmlHelper.Hidden(expression: null, value: model, htmlAttributes: htmlAttributesObject);
            if (viewData.ModelMetadata.HideSurroundingHtml)
            {
                return hidden;
            }

            return new HtmlContentBuilder(capacity: 2)
                .AppendHtml(display)
                .AppendHtml(hidden);
        }

        private static IDictionary<string, object> CreateHtmlAttributes(
            IHtmlHelper htmlHelper,
            string className,
            string inputType = null)
        {
            var htmlAttributesObject = htmlHelper.ViewData[HtmlAttributeKey];
            if (htmlAttributesObject != null)
            {
                return MergeHtmlAttributes(htmlAttributesObject, className, inputType);
            }

            var htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "class", className }
            };

            if (inputType != null)
            {
                htmlAttributes.Add("type", inputType);
            }

            return htmlAttributes;
        }

        private static IDictionary<string, object> MergeHtmlAttributes(
            object htmlAttributesObject,
            string className,
            string inputType)
        {
            var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributesObject);

            if (htmlAttributes.TryGetValue("class", out var htmlClassObject))
            {
                var htmlClassName = htmlClassObject + " " + className;
                htmlAttributes["class"] = htmlClassName;
            }
            else
            {
                htmlAttributes.Add("class", className);
            }

            // The input type from the provided htmlAttributes overrides the inputType parameter.
            if (inputType != null && !htmlAttributes.ContainsKey("type"))
            {
                htmlAttributes.Add("type", inputType);
            }

            return htmlAttributes;
        }

        public static async Task<IHtmlContent> MultilineTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(htmlHelper.TextArea(
                expression: string.Empty,
                value: htmlHelper.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString(),
                rows: 0,
                columns: 0,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "text-box multi-line")));
        }

        public static async Task<IHtmlContent> ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;

            if (templateInfo.TemplateDepth > 1)
            {
                if (modelExplorer.Model == null)
                {
                    return new HtmlString(modelExplorer.Metadata.NullDisplayText);
                }

                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    return new StringHtmlContent(text);
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
                    readOnly: false,
                    additionalViewData: null);

                var templateBuilderResult = await templateBuilder.Build();
                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = htmlHelper.Label(propertyMetadata.PropertyName, labelText: null, htmlAttributes: null);
                    using (var writer = new HasContentTextWriter())
                    {
                        label.WriteTo(writer, HtmlEncoder.Default);
                        if (writer.HasContent)
                        {
                            var labelTag = new TagBuilder("div");
                            labelTag.AddCssClass("editor-label");
                            labelTag.InnerHtml.SetHtmlContent(label);
                            content.AppendLine(labelTag);
                        }
                    }

                    var valueDivTag = new TagBuilder("div");
                    valueDivTag.AddCssClass("editor-field");

                    valueDivTag.InnerHtml.AppendHtml(templateBuilderResult);
                    valueDivTag.InnerHtml.AppendHtml(" ");
                    valueDivTag.InnerHtml.AppendHtml(htmlHelper.ValidationMessage(
                        propertyMetadata.PropertyName,
                        message: null,
                        htmlAttributes: null,
                        tag: null));

                    content.AppendLine(valueDivTag);
                }
                else
                {
                    content.AppendHtml(templateBuilderResult);
                }
            }

            return content;
        }

        public static async Task<IHtmlContent> PasswordTemplate(IHtmlHelper htmlHelper)
        {
            object value = null;
            if (AppContext.TryGetSwitch(UsePasswordValue, out var usePasswordValue) && usePasswordValue)
            {
                value = htmlHelper.ViewData.TemplateInfo.FormattedModelValue;
            }

            return await Task.FromResult(htmlHelper.Password(
                expression: null,
                value: value,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "text-box single-line password")));
        }

        private static bool ShouldShow(ModelExplorer modelExplorer, TemplateInfo templateInfo)
        {
            return
                modelExplorer.Metadata.ShowForEdit &&
                !modelExplorer.Metadata.IsComplexType &&
                !templateInfo.Visited(modelExplorer);
        }

        public static async Task<IHtmlContent> StringTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper));
        }

        public static async Task<IHtmlContent> PhoneNumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "tel"));
        }

        public static async Task<IHtmlContent> UrlInputTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "url"));
        }

        public static async Task<IHtmlContent> EmailAddressInputTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "email"));
        }

        public static async Task<IHtmlContent> DateTimeOffsetTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, @"{0:yyyy-MM-ddTHH\:mm\:ss.fffK}");
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "text"));
        }

        public static async Task<IHtmlContent> DateTimeLocalInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, @"{0:yyyy-MM-ddTHH\:mm\:ss.fff}");
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "datetime-local"));
        }

        public static async Task<IHtmlContent> DateInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-dd}");
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "date"));
        }

        public static async Task<IHtmlContent> TimeInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, @"{0:HH\:mm\:ss.fff}");
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "time"));
        }

        public static async Task<IHtmlContent> MonthInputTemplate(IHtmlHelper htmlHelper)
        {
            // "month" is a new HTML5 input type that only will be rendered in Rfc3339 mode
            htmlHelper.Html5DateRenderingMode = Html5DateRenderingMode.Rfc3339;
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM}");
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "month"));
        }

        public static async Task<IHtmlContent> WeekInputTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "week"));
        }

        public static async Task<IHtmlContent> NumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "number"));
        }

        public static async Task<IHtmlContent> FileInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return await Task.FromResult(GenerateTextBox(htmlHelper, inputType: "file"));
        }

        public static async Task<IHtmlContent> FileCollectionInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var htmlAttributes =
                CreateHtmlAttributes(htmlHelper, className: "text-box single-line", inputType: "file");
            htmlAttributes["multiple"] = "multiple";

            return await Task.FromResult(GenerateTextBox(htmlHelper, htmlHelper.ViewData.TemplateInfo.FormattedModelValue, htmlAttributes));
        }

        private static void ApplyRfc3339DateFormattingIfNeeded(IHtmlHelper htmlHelper, string format)
        {
            if (htmlHelper.Html5DateRenderingMode != Html5DateRenderingMode.Rfc3339)
            {
                return;
            }

            var metadata = htmlHelper.ViewData.ModelMetadata;
            var value = htmlHelper.ViewData.Model;
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue != value && metadata.HasNonDefaultEditFormat)
            {
                return;
            }

            if (value is DateTime || value is DateTimeOffset)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.InvariantCulture, format, value);
            }
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, string inputType = null)
        {
            return GenerateTextBox(htmlHelper, inputType, htmlHelper.ViewData.TemplateInfo.FormattedModelValue);
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, string inputType, object value)
        {
            var htmlAttributes =
                CreateHtmlAttributes(htmlHelper, className: "text-box single-line", inputType: inputType);

            return GenerateTextBox(htmlHelper, value, htmlAttributes);
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, object value, object htmlAttributes)
        {
            return htmlHelper.TextBox(
                expression: null,
                value: value,
                format: null,
                htmlAttributes: htmlAttributes);
        }

        private class HasContentTextWriter : TextWriter
        {
            public bool HasContent { get; private set; }

            public override Encoding Encoding => Null.Encoding;

            public override void Write(char value)
            {
                HasContent = true;
            }

            public override void Write(char[] buffer, int index, int count)
            {
                if (count > 0)
                {
                    HasContent = true;
                }
            }

            public override void Write(string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    HasContent = true;
                }
            }
        }
    }
}
