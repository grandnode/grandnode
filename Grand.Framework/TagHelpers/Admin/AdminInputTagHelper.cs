using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Grand.Framework.TagHelpers.Admin
{

    /// <summary>
    /// input tag helper
    /// </summary>
    [HtmlTargetElement("admin-input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class AdminInputTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string DisabledAttributeName = "asp-disabled";
        private const string RequiredAttributeName = "asp-required";
        private const string RenderFormControlClassAttributeName = "asp-render-form-control-class";
        private const string TemplateAttributeName = "asp-template";
        private const string PostfixAttributeName = "asp-postfix";
        private const string PostSelectItem = "asp-selectitem";

        private readonly IHtmlHelper _htmlHelper;

        /// <summary>
        /// An expression to be evaluated against the current model
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Indicates whether the field is disabled
        /// </summary>
        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        /// <summary>
        /// Indicates whether the field is required
        /// </summary>
        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        /// <summary>
        /// Indicates whether the "form-control" class shold be added to the input
        /// </summary>
        [HtmlAttributeName(RenderFormControlClassAttributeName)]
        public string RenderFormControlClass { set; get; }

        /// <summary>
        /// Editor template for the field
        /// </summary>
        [HtmlAttributeName(TemplateAttributeName)]
        public string Template { set; get; }

        [HtmlAttributeName(PostSelectItem)]
        public IList<SelectListItem> SelectItems { set; get; }

        /// <summary>
        /// Postfix
        /// </summary>
        [HtmlAttributeName(PostfixAttributeName)]
        public string Postfix { set; get; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        public AdminInputTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            //clear the output
            output.SuppressOutput();

            //disabled attribute
            bool.TryParse(IsDisabled, out bool disabled);
            if (disabled)
            {
                var d = new TagHelperAttribute("disabled", "disabled");
                output.Attributes.Add(d);
            }

            //required asterisk
            bool.TryParse(IsRequired, out bool required);
            if (required)
            {
                output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
                output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
            }

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //add form-control class
            bool.TryParse(RenderFormControlClass, out bool renderFormControlClass);
            object htmlAttributes = null;
            if (string.IsNullOrEmpty(RenderFormControlClass) && For.Metadata.ModelType.Name.Equals("String") || renderFormControlClass)
                htmlAttributes = new { @class = "form-control k-input" };

            var viewEngine = GetPrivateFieldValue(_htmlHelper, "_viewEngine") as IViewEngine;
            var bufferScope = GetPrivateFieldValue(_htmlHelper, "_bufferScope") as IViewBufferScope;
            if(SelectItems!=null)
            {
                if (SelectItems.Any())
                {
                    if (_htmlHelper.ViewData.ContainsKey("SelectList"))
                    {
                        _htmlHelper.ViewData["SelectList"] = SelectItems;
                    }
                    else
                    {
                        _htmlHelper.ViewData.Add("SelectList", SelectItems);
                    }
                }
                else
                {
                    if (_htmlHelper.ViewData.ContainsKey("SelectList"))
                    {
                        _htmlHelper.ViewData["SelectList"] = new List<SelectListItem>();
                    }
                    else
                    {
                        _htmlHelper.ViewData.Add("SelectList", new List<SelectListItem>());
                    }
                    
                }

            }
            var templateBuilder = new TemplateBuilder(
                viewEngine,
                bufferScope,
                _htmlHelper.ViewContext,
                _htmlHelper.ViewData,
                For.ModelExplorer,
                For.Name,
                Template,
                readOnly: false,
                additionalViewData: new { htmlAttributes, postfix = this.Postfix });

            var htmlOutput = templateBuilder.Build();
            output.Content.SetHtmlContent(htmlOutput.RenderHtmlContent());
        }



        private object GetPrivateFieldValue(object target, string fieldName)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "The assignment target cannot be null.");
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("fieldName", "The field name cannot be null or empty.");
            }

            var t = target.GetType();
            FieldInfo fi = null;

            while (t != null)
            {
                fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (fi != null) break;

                t = t.BaseType;
            }

            if (fi == null)
            {
                throw new Exception($"Field '{fieldName}' not found in type hierarchy.");
            }

            return fi.GetValue(target);
        }

    }


}