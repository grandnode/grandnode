using Grand.Core;
using Grand.Framework.UI.Paging;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Framework.TagHelpers
{
    [HtmlTargetElement("page-navigation", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("page-navigation", Attributes = QueryParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = PaginationParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowTotalSummaryParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowPagerItemsParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowFirstParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowPreviousParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowNextParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowLastParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = ShowIndividualPagesParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = RenderEmptyParametersParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = IndividualPagesDisplayedCountParamAttributeName)]
    [HtmlTargetElement("page-navigation", Attributes = BooleanParamAttributeName)]
    public class PageNavigationTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private const string QueryParamAttributeName = "asp-query-param";
        [HtmlAttributeName(QueryParamAttributeName)]
        public string QueryParam { get; set; } = "page";

        private const string PaginationParamAttributeName = "asp-pagination";
        [HtmlAttributeName(PaginationParamAttributeName)]
        public IPageableModel Pagination { get; set; }

        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IHtmlHelper _htmlHelper;

        private const string ShowTotalSummaryParamAttributeName = "asp-show-total-summary";
        [HtmlAttributeName(ShowTotalSummaryParamAttributeName)]
        public bool ShowTotalSummary { get; set; }

        private const string ShowPagerItemsParamAttributeName = "asp-show-pager-items";
        [HtmlAttributeName(ShowPagerItemsParamAttributeName)]
        public bool ShowPagerItems { get; set; } = true;

        private const string ShowFirstParamAttributeName = "asp-show-first";
        [HtmlAttributeName(ShowFirstParamAttributeName)]
        public bool ShowFirst { get; set; } = true;

        private const string ShowPreviousParamAttributeName = "asp-show-previous";
        [HtmlAttributeName(ShowPreviousParamAttributeName)]
        public bool ShowPrevious { get; set; } = true;

        private const string ShowNextParamAttributeName = "asp-show-next";
        [HtmlAttributeName(ShowNextParamAttributeName)]
        public bool ShowNext { get; set; } = true;

        private const string ShowLastParamAttributeName = "asp-show-last";
        [HtmlAttributeName(ShowLastParamAttributeName)]
        public bool ShowLast { get; set; } = true;

        private const string ShowIndividualPagesParamAttributeName = "asp-show-individual-pages";
        [HtmlAttributeName(ShowIndividualPagesParamAttributeName)]
        public bool ShowIndividualPages { get; set; } = true;


        private const string RenderEmptyParametersParamAttributeName = "asp-render-empty-parameters";
        [HtmlAttributeName(RenderEmptyParametersParamAttributeName)]
        public bool RenderEmptyParameters { get; set; } = true;

        private const string IndividualPagesDisplayedCountParamAttributeName = "asp-individual-pages-displayed-count";
        [HtmlAttributeName(IndividualPagesDisplayedCountParamAttributeName)]
        public int IndividualPagesDisplayedCount { get; set; } = 5;

        private const string BooleanParamAttributeName = "asp-boolean-params";
        [HtmlAttributeName(BooleanParamAttributeName)]
        public IList<string> BooleanParameterNames { get; set; } = new List<string>();

        public PageNavigationTagHelper(IWebHelper webHelper, ILocalizationService localizationService, IHtmlHelper htmlHelper)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _htmlHelper = htmlHelper;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            var links = GenerateHtmlString();

            if (!string.IsNullOrEmpty(links))
            {
                var sb = new StringBuilder();
                sb.AppendFormat("<nav aria-label=\"Page navigation\">");
                sb.Append(Environment.NewLine);
                sb.AppendFormat(links);
                sb.Append(Environment.NewLine);
                sb.AppendFormat("</nav>");
                sb.Append(Environment.NewLine);
                output.Content.SetHtmlContent(sb.ToString());
            }
            return Task.CompletedTask;
        }

        public virtual string GenerateHtmlString()
        {
            if (Pagination.TotalItems == 0)
                return null;

            var links = new StringBuilder();
            if (ShowTotalSummary && (Pagination.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(_localizationService.GetResource("Pager.CurrentPage"), Pagination.PageIndex + 1, Pagination.TotalPages, Pagination.TotalItems));
                links.Append("</li>");
            }
            if (ShowPagerItems && (Pagination.TotalPages > 1))
            {
                if (ShowFirst)
                {
                    //first page
                    if ((Pagination.PageIndex >= 3) && (Pagination.TotalPages > IndividualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(1, _localizationService.GetResource("Pager.First"), "first-page page-item"));
                    }
                }
                if (ShowPrevious)
                {
                    //previous page
                    if (Pagination.PageIndex > 0)
                    {
                        links.Append(CreatePageLink(Pagination.PageIndex, _localizationService.GetResource("Pager.Previous"), "previous-page page-item"));
                    }
                }
                if (ShowIndividualPages)
                {
                    //individual pages
                    int firstIndividualPageIndex = GetFirstIndividualPageIndex();
                    int lastIndividualPageIndex = GetLastIndividualPageIndex();
                    for (int i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (Pagination.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page page-item\"><a class=\"page-link\">{0}</a></li>", (i + 1));
                        }
                        else
                        {
                            links.Append(CreatePageLink(i + 1, (i + 1).ToString(), "individual-page page-item"));
                        }
                    }
                }
                if (ShowNext)
                {
                    //next page
                    if ((Pagination.PageIndex + 1) < Pagination.TotalPages)
                    {
                        links.Append(CreatePageLink(Pagination.PageIndex + 2, _localizationService.GetResource("Pager.Next"), "next-page page-item"));
                    }
                }
                if (ShowLast)
                {
                    //last page
                    if (((Pagination.PageIndex + 3) < Pagination.TotalPages) && (Pagination.TotalPages > IndividualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(Pagination.TotalPages, _localizationService.GetResource("Pager.Last"), "last-page page-item"));
                    }
                }
            }

            var result = links.ToString();
            if (!String.IsNullOrEmpty(result))
            {
                result = "<ul class=\"pagination\">" + result + "</ul>";
            }
            return result;
        }

        protected virtual string CreatePageLink(int pageNumber, string text, string cssClass)
        {
            var liBuilder = new TagBuilder("li");
            if (!string.IsNullOrWhiteSpace(cssClass))
                liBuilder.AddCssClass(cssClass);

            var aBuilder = new TagBuilder("a");
            aBuilder.InnerHtml.AppendHtml(text);
            aBuilder.AddCssClass("page-link");
            aBuilder.MergeAttribute("href", CreateDefaultUrl(pageNumber));

            liBuilder.InnerHtml.AppendHtml(aBuilder);
            return liBuilder.RenderHtmlContent();
        }
        protected virtual string CreateDefaultUrl(int pageNumber)
        {
            var routeValues = new RouteValueDictionary();

            var parametersWithEmptyValues = new List<string>();
            foreach (var key in _htmlHelper.ViewContext.HttpContext.Request.Query.Keys.Where(key => key != null))
            {
                //TODO test new implementation (QueryString, keys). And ensure no null exception is thrown when invoking ToString(). Is "StringValues.IsNullOrEmpty" required?
                var value = _htmlHelper.ViewContext.HttpContext.Request.Query[key].ToString();
                if (RenderEmptyParameters && string.IsNullOrEmpty(value))
                {
                    //we store query string parameters with empty values separately
                    //we need to do it because they are not properly processed in the UrlHelper.GenerateUrl method (dropped for some reasons)
                    parametersWithEmptyValues.Add(key);
                }
                else
                {
                    if (BooleanParameterNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                    {
                        //little hack here due to ugly MVC implementation
                        //find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
                        if (!string.IsNullOrEmpty(value) && value.Equals("true,false", StringComparison.OrdinalIgnoreCase))
                        {
                            value = "true";
                        }
                    }
                    routeValues[key] = value;
                }
            }

            if (pageNumber > 1)
            {
                routeValues[QueryParam] = pageNumber;
            }
            else
            {
                //SEO. we do not render pageindex query string parameter for the first page
                if (routeValues.ContainsKey(QueryParam))
                {
                    routeValues.Remove(QueryParam);
                }
            }

            var url = _webHelper.GetThisPageUrl(false);
            foreach (var routeValue in routeValues)
            {
                url = _webHelper.ModifyQueryString(url, routeValue.Key, routeValue.Value?.ToString());
            }
            if (RenderEmptyParameters && parametersWithEmptyValues.Any())
            {
                foreach (var key in parametersWithEmptyValues)
                {
                    url = _webHelper.ModifyQueryString(url, key, null);
                }
            }
            return url;
        }

        protected virtual int GetFirstIndividualPageIndex()
        {
            if ((Pagination.TotalPages < IndividualPagesDisplayedCount) ||
                ((Pagination.PageIndex - (IndividualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((Pagination.PageIndex + (IndividualPagesDisplayedCount / 2)) >= Pagination.TotalPages)
            {
                return (Pagination.TotalPages - IndividualPagesDisplayedCount);
            }
            return (Pagination.PageIndex - (IndividualPagesDisplayedCount / 2));
        }
        protected virtual int GetLastIndividualPageIndex()
        {
            int num = IndividualPagesDisplayedCount / 2;
            if ((IndividualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((Pagination.TotalPages < IndividualPagesDisplayedCount) ||
                ((Pagination.PageIndex + num) >= Pagination.TotalPages))
            {
                return (Pagination.TotalPages - 1);
            }
            if ((Pagination.PageIndex - (IndividualPagesDisplayedCount / 2)) < 0)
            {
                return (IndividualPagesDisplayedCount - 1);
            }
            return (Pagination.PageIndex + num);
        }
    }
}
