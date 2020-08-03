using Grand.Framework.Localization;
using Grand.Services.Localization;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Grand.Framework
{
    public static class HtmlExtensions
    {
        #region Admin area extensions

        public static async Task<IHtmlContent> LocalizedEditor<T, TLocalizedModelLocal>(this IHtmlHelper<T> helper,
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
                var storeService = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IStoreService>();
                if ((await storeService.GetAllStores()).Count >= 2)
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

                var languageService = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<ILanguageService>();
                foreach (var locale in helper.ViewData.Model.Locales)
                {
                    //languages
                    var language = await languageService.GetLanguageById(locale.LanguageId);

                    tabStrip.AppendLine("<li>");
                    var urlHelper = new UrlHelper(helper.ViewContext);
                    var iconUrl = urlHelper.Content("~/content/images/flags/" + language.FlagImageFileName);
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
                result.Append("<label class=\"mt-checkbox control control-checkbox\">");
                var check = helper.CheckBoxFor(expression, new Dictionary<string, object>
                {
                    { "class", cssClass },
                    { "onclick", onClick },
                    { "data-for-input-selector", dataInputSelector },
                });
                result.Append(check.ToHtmlString());
                result.Append("<div class='control__indicator'></div>");
                result.Append("</label>");
            }
            return new HtmlString(result.ToString());
        }

        public static string FieldIdFor<T, TResult>(this IHtmlHelper<T> html, Expression<Func<T, TResult>> expression)
        {
            //TO DO remove this method and use in cshtml files
            return html.IdFor(expression);
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
        #endregion

        #region Common extensions

        public static string ToHtmlString(this IHtmlContent tag)
        {
            using (var writer = new StringWriter())
            {
                tag.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        #endregion
    }
}

