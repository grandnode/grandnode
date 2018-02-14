using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Grand.Core;
using Grand.Core.Infrastructure;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Html;
using Grand.Framework.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;
using System.IO;
using System.Text.Encodings.Web;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Grand.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Grand.Framework
{
    public static class HtmlExtensions
    {
        #region Admin area extensions

        public static IHtmlContent LocalizedEditor<T, TLocalizedModelLocal>(this IHtmlHelper<T> helper,
            string name,
            Func<int, HelperResult> localizedTemplate,
            Func<T, HelperResult> standardTemplate,
            bool ignoreIfSeveralStores = false)
            where T : ILocalizedModel<TLocalizedModelLocal>
            where TLocalizedModelLocal : ILocalizedModelLocal
        {
            
            var localizationSupported = helper.ViewData.Model.Locales.Count > 1;
            if (ignoreIfSeveralStores)
            {
                var storeService = EngineContext.Current.Resolve<IStoreService>();
                if (storeService.GetAllStores().Count >= 2)
                {
                    localizationSupported = false;
                }
            }
            if (localizationSupported)
            {
                var tabStrip = new StringBuilder();
                tabStrip.AppendLine(string.Format("<div id='{0}'>", name));
                tabStrip.AppendLine("<ul>");

                //default tab
                tabStrip.AppendLine("<li class='k-state-active'>");
                tabStrip.AppendLine("Standard");
                tabStrip.AppendLine("</li>");

                foreach (var locale in helper.ViewData.Model.Locales)
                {
                    //languages
                    var language = EngineContext.Current.Resolve<ILanguageService>().GetLanguageById(locale.LanguageId);

                    tabStrip.AppendLine("<li>");
                    var urlHelper = new UrlHelper(helper.ViewContext);
                    var iconUrl = urlHelper.Content("~/Content/Images/flags/" + language.FlagImageFileName);
                    tabStrip.AppendLine(string.Format("<img class='k-image' alt='' src='{0}'>", iconUrl));
                    tabStrip.AppendLine(WebUtility.HtmlEncode(language.Name));
                    tabStrip.AppendLine("</li>");
                }
                tabStrip.AppendLine("</ul>");



                //default tab
                tabStrip.AppendLine("<div>");
                tabStrip.AppendLine(standardTemplate(helper.ViewData.Model).ToHtmlString());
                tabStrip.AppendLine("</div>");

                for (int i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                {
                    //languages
                    tabStrip.AppendLine("<div>");
                    tabStrip.AppendLine(localizedTemplate(i).ToHtmlString());
                    tabStrip.AppendLine("</div>");
                }
                tabStrip.AppendLine("</div>");
                tabStrip.AppendLine("<script>");
                tabStrip.AppendLine("$(document).ready(function() {");
                tabStrip.AppendLine(string.Format("$('#{0}').kendoTabStrip(", name));
                tabStrip.AppendLine("{");
                tabStrip.AppendLine("animation:  {");
                tabStrip.AppendLine("open: {");
                tabStrip.AppendLine("effects: \"fadeIn\"");
                tabStrip.AppendLine("}");
                tabStrip.AppendLine("}");
                tabStrip.AppendLine("});");
                tabStrip.AppendLine("});");
                tabStrip.AppendLine("</script>");
                return new HtmlString(tabStrip.ToString());
            }
            else
            {
                return standardTemplate(helper.ViewData.Model);
            }
        }

        public static IHtmlContent DeleteConfirmation<T>(this IHtmlHelper<T> helper, string buttonsSelector) where T : BaseGrandEntityModel
        {
            return DeleteConfirmation(helper, "", buttonsSelector);
        }

        public static IHtmlContent DeleteConfirmation<T>(this IHtmlHelper<T> helper, string actionName,
                    string buttonsSelector) where T : BaseGrandEntityModel
        {
            if (String.IsNullOrEmpty(actionName))
                actionName = "Delete";

            var modalId = new HtmlString(helper.ViewData.ModelMetadata.ModelType.Name.ToLower() + "-delete-confirmation").ToHtmlString();


            var deleteConfirmationModel = new DeleteConfirmationModel
            {
                Id = helper.ViewData.Model.Id.ToString(),
                ControllerName = helper.ViewContext.RouteData.Values["controller"].ToString(),
                ActionName = actionName,
                WindowId = modalId
            };

            var window = new StringBuilder();
            window.AppendLine(string.Format("<div id='{0}' style='display:none;'>", modalId));
            window.AppendLine(helper.Partial("Delete", deleteConfirmationModel).ToHtmlString());
            window.AppendLine("</div>");
            window.AppendLine("<script>");
            window.AppendLine("$(document).ready(function() {");
            window.AppendLine(string.Format("$('#{0}').click(function (e) ", buttonsSelector));
            window.AppendLine("{");
            window.AppendLine("e.preventDefault();");
            window.AppendLine(string.Format("var window = $('#{0}');", modalId));
            window.AppendLine("if (!window.data('kendoWindow')) {");
            window.AppendLine("window.kendoWindow({");
            window.AppendLine("modal: true,");
            window.AppendLine(string.Format("title: '{0}',", EngineContext.Current.Resolve<ILocalizationService>().GetResource("Admin.Common.AreYouSure")));
            window.AppendLine("actions: ['Close']");
            window.AppendLine("});");
            window.AppendLine("}");
            window.AppendLine("window.data('kendoWindow').center().open();");
            window.AppendLine("});");
            window.AppendLine("});");
            window.AppendLine("</script>");

            return new HtmlString(window.ToString());
        }

        public static IHtmlContent GrandLabelFor<TModel, TValue>(
            this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            IDictionary<string, object> htmlAttributes = null,
            bool withColumns = true)
        {
            var metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string labelText = metadata.Metadata.DisplayName ?? metadata.Metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
            {
                return HtmlString.Empty;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName));
            if (withColumns)
                tag.AddCssClass("control-label col-md-3 col-sm-3");
            tag.InnerHtml.SetHtmlContent(labelText);

            object value;
            var hintResource = string.Empty;
            if (metadata.Metadata.AdditionalValues.TryGetValue("GrandResourceDisplayNameAttribute", out value))
            {
                var resourceDisplayName = value as GrandResourceDisplayNameAttribute;
                if (resourceDisplayName != null)
                {
                    var langId = EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.Id;
                    hintResource = EngineContext.Current.Resolve<ILocalizationService>()
                        .GetResource(resourceDisplayName.ResourceKey + ".Hint", langId);
                    if (!String.IsNullOrEmpty(hintResource))
                    {
                        TagBuilder i = new TagBuilder("i");
                        i.AddCssClass("help icon-question");
                        i.Attributes.Add("title", hintResource);
                        i.Attributes.Add("data-toggle", "tooltip");
                        i.Attributes.Add("data-placement", "bottom");
                        tag.InnerHtml.SetHtmlContent(string.Format("{0} {1}", labelText, i.ToHtmlString()));
                    }
                }
            }
            return new HtmlString(tag.ToHtmlString());
        }

        public static IHtmlContent GrandEditorFor<TModel, TValue>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, string postfix = "",
            bool? renderFormControlClass = null, bool required = false)
        {
            object htmlAttributes = null;
            var metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider);
            if ((!renderFormControlClass.HasValue && metadata.ModelType.Name.Equals("String")) ||
                (renderFormControlClass.HasValue && renderFormControlClass.Value))
                htmlAttributes = new { @class = "form-control" };

            string result = "";
            var editorHtml = helper.EditorFor(expression, new { htmlAttributes, postfix }).ToHtmlString();
            if (required)
                result = string.Format("<div class=\"input-group input-group-required\">{0}<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>", editorHtml);
            else
                result = editorHtml;

            return new HtmlString(result);
        }

        public static IHtmlContent GrandDropDownListFor<TModel, TValue>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> itemList,
            object htmlAttributes = null)
        {
            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attrs = AddFormControlClassToHtmlAttributes(attrs);
            return helper.DropDownListFor(expression, itemList, attrs);
        }

        public static IHtmlContent GrandDropDownList<TModel>(this IHtmlHelper<TModel> helper, string name,
            IEnumerable<SelectListItem> itemList, object htmlAttributes = null,
            bool renderFormControlClass = true, bool required = false)
        {
            var result = new StringBuilder();

            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (renderFormControlClass)
                attrs = AddFormControlClassToHtmlAttributes(attrs);

            if (required)
                result.AppendFormat("<div class=\"input-group input-group-required\">{0}<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>",
                    helper.DropDownList(name, itemList, attrs).ToHtmlString());
            else
                result.Append(helper.DropDownList(name, itemList, attrs).ToHtmlString());

            return new HtmlString(result.ToString());
        }

        public static IHtmlContent GrandTextAreaFor<TModel, TValue>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression, object htmlAttributes = null,
            bool renderFormControlClass = true, int rows = 4, int columns = 20, bool required = false)
        {
            var result = new StringBuilder();

            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (renderFormControlClass)
                attrs = AddFormControlClassToHtmlAttributes(attrs);

            if (required)
                result.AppendFormat("<div class=\"input-group input-group-required\">{0}<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>",
                    helper.TextAreaFor(expression, rows, columns, attrs).ToHtmlString());
            else
                result.Append(helper.TextAreaFor(expression, rows, columns, attrs).ToHtmlString());

            return new HtmlString(result.ToString());
        }


        public static RouteValueDictionary AddFormControlClassToHtmlAttributes(IDictionary<string, object> htmlAttributes)
        {
            if (!htmlAttributes.ContainsKey("class") || htmlAttributes["class"] == null || string.IsNullOrEmpty(htmlAttributes["class"].ToString()))
                htmlAttributes["class"] = "form-control";
            else
                if (!htmlAttributes["class"].ToString().Contains("form-control"))
                htmlAttributes["class"] += " form-control";

            return new RouteValueDictionary(htmlAttributes);
        }

        public static IHtmlContent OverrideStoreCheckboxFor<TModel, TValue>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TValue>> forInputExpression,
            string activeStoreScopeConfiguration)
        {
            var dataInputIds = new List<string>();
            dataInputIds.Add(helper.FieldIdFor(forInputExpression));
            return OverrideStoreCheckboxFor(helper, expression, activeStoreScopeConfiguration, null, dataInputIds.ToArray());
        }
        public static IHtmlContent OverrideStoreCheckboxFor<TModel, TValue1, TValue2>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TValue1>> forInputExpression1,
            Expression<Func<TModel, TValue2>> forInputExpression2,
            string activeStoreScopeConfiguration)
        {
            var dataInputIds = new List<string>();
            dataInputIds.Add(helper.FieldIdFor(forInputExpression1));
            dataInputIds.Add(helper.FieldIdFor(forInputExpression2));
            return OverrideStoreCheckboxFor(helper, expression, activeStoreScopeConfiguration, null, dataInputIds.ToArray());
        }
        public static IHtmlContent OverrideStoreCheckboxFor<TModel, TValue1, TValue2, TValue3>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, bool>> expression,
            Expression<Func<TModel, TValue1>> forInputExpression1,
            Expression<Func<TModel, TValue2>> forInputExpression2,
            Expression<Func<TModel, TValue3>> forInputExpression3,
            string activeStoreScopeConfiguration)
        {
            var dataInputIds = new List<string>();
            dataInputIds.Add(helper.FieldIdFor(forInputExpression1));
            dataInputIds.Add(helper.FieldIdFor(forInputExpression2));
            dataInputIds.Add(helper.FieldIdFor(forInputExpression3));
            return OverrideStoreCheckboxFor(helper, expression, activeStoreScopeConfiguration, null, dataInputIds.ToArray());
        }
        public static IHtmlContent OverrideStoreCheckboxFor<TModel>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, bool>> expression,
            string parentContainer,
            string activeStoreScopeConfiguration)
        {
            return OverrideStoreCheckboxFor(helper, expression, activeStoreScopeConfiguration, parentContainer, null);
        }
        private static IHtmlContent OverrideStoreCheckboxFor<TModel>(this IHtmlHelper<TModel> helper,
            Expression<Func<TModel, bool>> expression,
            string activeStoreScopeConfiguration = "",
            string parentContainer = null,
            params string[] datainputIds)
        {
            if (String.IsNullOrEmpty(parentContainer) && datainputIds == null)
                throw new ArgumentException("Specify at least one selector");

            var result = new StringBuilder();
            if (!String.IsNullOrEmpty(activeStoreScopeConfiguration))
            {
                //render only when a certain store is chosen
                const string cssClass = "multi-store-override-option";
                string dataInputSelector = "";
                if (!String.IsNullOrEmpty(parentContainer))
                {
                    dataInputSelector = "#" + parentContainer + " input, #" + parentContainer + " textarea, #" + parentContainer + " select";
                }
                if (datainputIds != null && datainputIds.Length > 0)
                {
                    dataInputSelector = "#" + String.Join(", #", datainputIds);
                }
                var onClick = string.Format("checkOverriddenStoreValue(this, '{0}')", dataInputSelector);
                result.Append("<label class=\"mt-checkbox\">");
                var check = helper.CheckBoxFor(expression, new Dictionary<string, object>
                {
                    { "class", cssClass },
                    { "onclick", onClick },
                    { "data-for-input-selector", dataInputSelector },
                });
                result.Append(check.ToHtmlString());
                result.Append("<span></span>");
                result.Append("</label>");
            }
            return new HtmlString(result.ToString());
        }

        public static string FieldNameFor<T, TResult>(this IHtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            //TO DO remove this method and use in cshtml files
            return html.NameFor(expression);
        }
        public static string FieldIdFor<T, TResult>(this IHtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            //TO DO remove this method and use in cshtml files
            return html.IdFor(expression);
        }

        public static IHtmlContent Hint(this IHtmlHelper helper, string value)
        {
            //create tag builder
            var builder = new TagBuilder("i");
            builder.MergeAttribute("title", value);
            builder.MergeAttribute("class", "ico-question");
            var icon = new StringBuilder();
            icon.Append("<i class='fa fa-question-circle'></i>");
            builder.InnerHtml.AppendHtml(icon.ToString());
            //render tag
            return new HtmlString(builder.ToHtmlString());

        }
        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var htmlOutput = writer.ToString();
                return htmlOutput;
            }
        }
        /// <summary>
        /// Render CSS styles of selected index 
        /// </summary>
        /// <param name="helper">HTML helper</param>
        /// <param name="currentIndex">Current tab index (where appropriate CSS style should be rendred)</param>
        /// <param name="indexToSelect">Tab index to select</param>
        /// <returns>MvcHtmlString</returns>
        public static IHtmlContent RenderSelectedTabIndex(this IHtmlHelper helper, int currentIndex, int indexToSelect)
        {
            if (helper == null)
                throw new ArgumentNullException("helper");

            //ensure it's not negative
            if (indexToSelect < 0)
                indexToSelect = 0;

            //required validation
            if (indexToSelect == currentIndex)
            {
                return new HtmlString(" class='k-state-active'");
            }

            return new HtmlString("");
        }

        #endregion

        #region Common extensions

        public static IHtmlContent RequiredHint(this IHtmlHelper helper, string additionalText = null)
        {
            // Create tag builder
            var tagBuilder = new TagBuilder("span");
            tagBuilder.AddCssClass("required");
            var innerText = "*";
            //add additional text if specified
            if (!String.IsNullOrEmpty(additionalText))
                innerText += " " + additionalText;
            tagBuilder.InnerHtml.AppendHtml(innerText);
            return new HtmlString(tagBuilder.RenderHtmlContent());
        }

        public static string ToHtmlString(this IHtmlContent tag)
        {
            using (var writer = new StringWriter())
            {
                tag.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }

        /// <summary>
        /// Creates a days, months, years drop down list using an HTML select control. 
        /// The parameters represent the value of the "name" attribute on the select control.
        /// </summary>
        /// <param name="html">HTML helper</param>
        /// <param name="dayName">"Name" attribute of the day drop down list.</param>
        /// <param name="monthName">"Name" attribute of the month drop down list.</param>
        /// <param name="yearName">"Name" attribute of the year drop down list.</param>
        /// <param name="beginYear">Begin year</param>
        /// <param name="endYear">End year</param>
        /// <param name="selectedDay">Selected day</param>
        /// <param name="selectedMonth">Selected month</param>
        /// <param name="selectedYear">Selected year</param>
        /// <param name="localizeLabels">Localize labels</param>
        /// <param name="htmlAttributes">HTML attributes</param>
        /// <returns></returns>
        public static IHtmlContent DatePickerDropDowns(this IHtmlHelper html,
            string dayName, string monthName, string yearName,
            int? beginYear = null, int? endYear = null,
            int? selectedDay = null, int? selectedMonth = null, int? selectedYear = null,
            bool localizeLabels = true, object htmlAttributes = null, bool wrapTags = false)
        {
            var daysList = new TagBuilder("select");
            var monthsList = new TagBuilder("select");
            var yearsList = new TagBuilder("select");

            daysList.Attributes.Add("name", dayName);
            monthsList.Attributes.Add("name", monthName);
            yearsList.Attributes.Add("name", yearName);

            var htmlAttributesDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            daysList.MergeAttributes(htmlAttributesDictionary, true);
            monthsList.MergeAttributes(htmlAttributesDictionary, true);
            yearsList.MergeAttributes(htmlAttributesDictionary, true);

            var days = new StringBuilder();
            var months = new StringBuilder();
            var years = new StringBuilder();

            string dayLocale, monthLocale, yearLocale;
            if (localizeLabels)
            {
                var locService = EngineContext.Current.Resolve<ILocalizationService>();
                dayLocale = locService.GetResource("Common.Day");
                monthLocale = locService.GetResource("Common.Month");
                yearLocale = locService.GetResource("Common.Year");
            }
            else
            {
                dayLocale = "Day";
                monthLocale = "Month";
                yearLocale = "Year";
            }

            days.AppendFormat("<option value='{0}'>{1}</option>", "0", dayLocale);
            for (int i = 1; i <= 31; i++)
                days.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                    (selectedDay.HasValue && selectedDay.Value == i) ? " selected=\"selected\"" : null);


            months.AppendFormat("<option value='{0}'>{1}</option>", "0", monthLocale);
            for (int i = 1; i <= 12; i++)
            {
                months.AppendFormat("<option value='{0}'{1}>{2}</option>",
                                    i,
                                    (selectedMonth.HasValue && selectedMonth.Value == i) ? " selected=\"selected\"" : null,
                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetMonthName(i));
            }


            years.AppendFormat("<option value='{0}'>{1}</option>", "0", yearLocale);

            if (beginYear == null)
                beginYear = DateTime.UtcNow.Year - 100;
            if (endYear == null)
                endYear = DateTime.UtcNow.Year;

            if (endYear > beginYear)
            {
                for (int i = beginYear.Value; i <= endYear.Value; i++)
                    years.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                        (selectedYear.HasValue && selectedYear.Value == i) ? " selected=\"selected\"" : null);
            }
            else
            {
                for (int i = beginYear.Value; i >= endYear.Value; i--)
                    years.AppendFormat("<option value='{0}'{1}>{0}</option>", i,
                        (selectedYear.HasValue && selectedYear.Value == i) ? " selected=\"selected\"" : null);
            }

            daysList.InnerHtml.AppendHtml(days.ToString());
            monthsList.InnerHtml.AppendHtml(months.ToString());
            yearsList.InnerHtml.AppendHtml(years.ToString());

            if (wrapTags)
            {
                string wrapDaysList = "<span class=\"days-list select-wrapper\">" + daysList.RenderHtmlContent() + "</span>";
                string wrapMonthsList = "<span class=\"months-list select-wrapper\">" + monthsList.RenderHtmlContent() + "</span>";
                string wrapYearsList = "<span class=\"years-list select-wrapper\">" + yearsList.RenderHtmlContent() + "</span>";

                return new HtmlString(string.Concat(wrapDaysList, wrapMonthsList, wrapYearsList));
            }
            else
            {
                return new HtmlString(string.Concat(daysList.RenderHtmlContent(), monthsList.RenderHtmlContent(), yearsList.RenderHtmlContent()));
            }

        }

        /// <summary>
        /// Renders the standard label with a specified suffix added to label text
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <typeparam name="TValue">Value</typeparam>
        /// <param name="html">HTML helper</param>
        /// <param name="expression">Expression</param>
        /// <param name="htmlAttributes">HTML attributes</param>
        /// <param name="suffix">Suffix</param>
        /// <returns>Label</returns>
        public static IHtmlContent LabelFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes, string suffix)
        {
            //TODO refactor the way it's implemented in \Microsoft.AspNetCore.Mvc.ViewFeatures\ViewFeatures\HtmlHelperOfT.cs - "IHtmlContent LabelFor<TResult>"
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);

            var metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            string resolvedLabelText = metadata.Metadata.DisplayName ?? (metadata.Metadata.PropertyName ?? htmlFieldName.Split(new[] { '.' }).Last());
            if (string.IsNullOrEmpty(resolvedLabelText))
            {
                return new HtmlString("");
            }
            var tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.IdFor(expression), ""));
            if (!String.IsNullOrEmpty(suffix))
            {
                resolvedLabelText = String.Concat(resolvedLabelText, suffix);
            }
            tag.InnerHtml.AppendHtml(resolvedLabelText);

            var dictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            tag.MergeAttributes(dictionary, true);

            return new HtmlString(tag.ToHtmlString());

        }

        #endregion
    }
}

