using Grand.Core;
using Grand.Core.Infrastructure;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace Grand.Framework.UI.Paging
{
    /// <summary>
    /// Renders a pager component from an IPageableModel datasource.
    /// </summary>
    public partial class Pager : IHtmlContent
	{
        protected readonly IPageableModel model;
        protected readonly ViewContext viewContext;
        protected string pageQueryName = "page";
        protected bool showTotalSummary;
        protected bool showPagerItems = true;
        protected bool showFirst = true;
        protected bool showPrevious = true;
        protected bool showNext = true;
        protected bool showLast = true;
        protected bool showIndividualPages = true;
        protected bool renderEmptyParameters = true;
        protected int individualPagesDisplayedCount = 5;
        protected IList<string> booleanParameterNames;

		public Pager(IPageableModel model, ViewContext context)
		{
            this.model = model;
            this.viewContext = context;
            this.booleanParameterNames = new List<string>();
		}

		protected ViewContext ViewContext 
		{
			get { return viewContext; }
		}
        
        public Pager QueryParam(string value)
		{
            this.pageQueryName = value;
			return this;
		}
        public Pager ShowTotalSummary(bool value)
        {
            this.showTotalSummary = value;
            return this;
        }
        public Pager ShowPagerItems(bool value)
        {
            this.showPagerItems = value;
            return this;
        }
        public Pager ShowFirst(bool value)
        {
            this.showFirst = value;
            return this;
        }
        public Pager ShowPrevious(bool value)
        {
            this.showPrevious = value;
            return this;
        }
        public Pager ShowNext(bool value)
        {
            this.showNext = value;
            return this;
        }
        public Pager ShowLast(bool value)
        {
            this.showLast = value;
            return this;
        }
        public Pager ShowIndividualPages(bool value)
        {
            this.showIndividualPages = value;
            return this;
        }
        public Pager RenderEmptyParameters(bool value)
        {
            this.renderEmptyParameters = value;
            return this;
        }
        public Pager IndividualPagesDisplayedCount(int value)
        {
            this.individualPagesDisplayedCount = value;
            return this;
        }
        //little hack here due to ugly MVC implementation
        //find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
        public Pager BooleanParameterName(string paramName)
        {
            booleanParameterNames.Add(paramName);
            return this;
        }

	    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
	    {
            var htmlString = GenerateHtmlString();
	        writer.Write(htmlString);
	    }
	    public override string ToString()
	    {
	        return GenerateHtmlString();
	    }
        public virtual string GenerateHtmlString()
		{
            if (model.TotalItems == 0)
                return null;
            var localizationService = viewContext.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();

            var links = new StringBuilder();
            if (showTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(localizationService.GetResource("Pager.CurrentPage"), model.PageIndex + 1, model.TotalPages, model.TotalItems));
                links.Append("</li>");
            }
            if (showPagerItems && (model.TotalPages > 1))
            {
                if (showFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(1, localizationService.GetResource("Pager.First"), "first-page"));
                    }
                }
                if (showPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        links.Append(CreatePageLink(model.PageIndex, localizationService.GetResource("Pager.Previous"), "previous-page page-item"));
                    }
                }
                if (showIndividualPages)
                {
                    //individual pages
                    int firstIndividualPageIndex = GetFirstIndividualPageIndex();
                    int lastIndividualPageIndex = GetLastIndividualPageIndex();
                    for (int i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page page-item\"><a class=\"page-link\">{0}</a></li>", (i + 1));
                        }
                        else
                        {
                            links.Append(CreatePageLink(i + 1, (i + 1).ToString(), "individual-page page-item"));
                        }
                    }
                }
                if (showNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        links.Append(CreatePageLink(model.PageIndex + 2, localizationService.GetResource("Pager.Next"), "next-page page-item"));
                    }
                }
                if (showLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLink(model.TotalPages, localizationService.GetResource("Pager.Last"), "last-page page-item"));
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
	    public virtual bool IsEmpty()
	    {
            var html = GenerateHtmlString();
	        return string.IsNullOrEmpty(html);
	    }

        protected virtual int GetFirstIndividualPageIndex()
        {
            if ((model.TotalPages < individualPagesDisplayedCount) ||
                ((model.PageIndex - (individualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((model.PageIndex + (individualPagesDisplayedCount / 2)) >= model.TotalPages)
            {
                return (model.TotalPages - individualPagesDisplayedCount);
            }
            return (model.PageIndex - (individualPagesDisplayedCount / 2));
        }
        protected virtual int GetLastIndividualPageIndex()
        {
            int num = individualPagesDisplayedCount / 2;
            if ((individualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((model.TotalPages < individualPagesDisplayedCount) ||
                ((model.PageIndex + num) >= model.TotalPages))
            {
                return (model.TotalPages - 1);
            }
            if ((model.PageIndex - (individualPagesDisplayedCount / 2)) < 0)
            {
                return (individualPagesDisplayedCount - 1);
            }
            return (model.PageIndex + num);
        }
		protected virtual string CreatePageLink(int pageNumber, string text, string cssClass)
		{
            var liBuilder = new TagBuilder("li");
            if (!String.IsNullOrWhiteSpace(cssClass))
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
			foreach (var key in viewContext.HttpContext.Request.Query.Keys.Where(key => key != null))
			{
			    //TODO test new implementation (QueryString, keys). And ensure no null exception is thrown when invoking ToString(). Is "StringValues.IsNullOrEmpty" required?
                var value = viewContext.HttpContext.Request.Query[key].ToString();
                if (renderEmptyParameters && String.IsNullOrEmpty(value))
			    {
                    //we store query string parameters with empty values separately
                    //we need to do it because they are not properly processed in the UrlHelper.GenerateUrl method (dropped for some reasons)
                    parametersWithEmptyValues.Add(key);
			    }
			    else
                {
                    if (booleanParameterNames.Contains(key, StringComparer.OrdinalIgnoreCase))
                    {
                        //little hack here due to ugly MVC implementation
                        //find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
                        if (!String.IsNullOrEmpty(value) && value.Equals("true,false", StringComparison.OrdinalIgnoreCase))
                        {
                            value = "true";
                        }
                    }
                    routeValues[key] = value;
			    }
			}

            if (pageNumber > 1)
            {
                routeValues[pageQueryName] = pageNumber;
            }
            else
            {
                //SEO. we do not render pageindex query string parameter for the first page
                if (routeValues.ContainsKey(pageQueryName))
                {
                    routeValues.Remove(pageQueryName);
                }
            }

		    var webHelper = viewContext.HttpContext.RequestServices.GetRequiredService<IWebHelper>();
		    var url = webHelper.GetThisPageUrl(false);
		    foreach (var routeValue in routeValues)
		    {
		        url = webHelper.ModifyQueryString(url, routeValue.Key, routeValue.Value?.ToString());
		    }
            if (renderEmptyParameters && parametersWithEmptyValues.Any())
            {
                foreach (var key in parametersWithEmptyValues)
                {
                    url = webHelper.ModifyQueryString(url, key, null);
                }
            }
			return url;
		}

    }
}