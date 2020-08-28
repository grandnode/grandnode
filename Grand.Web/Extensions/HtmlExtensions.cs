using Grand.Framework;
using Grand.Framework.UI.Paging;
using Grand.Services.Localization;
using Grand.Web.Models.Boards;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;

namespace Grand.Web.Extensions
{
    public static class HtmlExtensions
    {

        //we have two pagers:
        //The first one can have custom routes
        //The second one just adds query string parameter
        public static IHtmlContent Pager<TModel>(this IHtmlHelper<TModel> html, ILocalizationService localizationService, PagerModel model)
        {
            if (model.TotalRecords == 0)
                return null;

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary page-item\">");
                links.Append(string.Format(model.CurrentPageText, model.PageIndex + 1, model.TotalPages, model.TotalRecords));
                links.Append("</li>");
            }
            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = 1;

                        links.Append("<li class=\"first-page page-item\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.FirstPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.FirstButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.FirstPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.pageNumber = (model.PageIndex);

                        links.Append("<li class=\"previous-page page-item\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.PreviousPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.PreviousButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.PreviousPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowIndividualPages)
                {
                    //individual pages
                    int firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    int lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (int i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page page-item\"><a class=\"page-link\">{0}</a></li>", (i + 1));
                        }
                        else
                        {
                            model.RouteValues.pageNumber = (i + 1);

                            links.Append("<li class=\"individual-page page-item\">");
                            if (model.UseRouteLinks)
                            {
                                links.Append(html.RouteLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), (i + 1)), @class = "page-link" }).ToHtmlString());
                            }
                            else
                            {
                                links.Append(html.ActionLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), (i + 1)), @class = "page-link" }).ToHtmlString());
                            }
                            links.Append("</li>");
                        }
                    }
                }
                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.pageNumber = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page page-item\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.NextPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.NextButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.NextPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.pageNumber = model.TotalPages;

                        links.Append("<li class=\"last-page page-item\">");
                        if (model.UseRouteLinks)
                        {
                            links.Append(html.RouteLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.LastPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        else
                        {
                            links.Append(html.ActionLink(model.LastButtonText, model.RouteActionName, model.RouteValues, new { title = localizationService.GetResource("Pager.LastPageTitle"), @class = "page-link" }).ToHtmlString());
                        }
                        links.Append("</li>");
                    }
                }
            }
            var result = links.ToString();
            if (!String.IsNullOrEmpty(result))
            {
                result = "<ul class=\"pagination\">" + result + "</ul>";
            }
            return new HtmlString(result);
        }
        public static IHtmlContent ForumTopicSmallPager<TModel>(this IHtmlHelper<TModel> html, ILocalizationService localizationService, ForumTopicRowModel model)
        {
            var forumTopicId = model.Id;
            var forumTopicSlug = model.SeName;
            var totalPages = model.TotalPostPages;

            if (totalPages > 0)
            {
                var links = new StringBuilder();

                if (totalPages <= 4)
                {
                    for (int x = 1; x <= totalPages; x++)
                    {
                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, pageNumber = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }).ToHtmlString());
                        if (x < totalPages)
                        {
                            links.Append(", ");
                        }
                    }
                }
                else
                {
                    links.Append(html.RouteLink("1", "TopicSlugPaged", new { id = forumTopicId, pageNumber = (1), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), 1) }).ToHtmlString());
                    links.Append(" ... ");

                    for (int x = (totalPages - 2); x <= totalPages; x++)
                    {
                        links.Append(html.RouteLink(x.ToString(), "TopicSlugPaged", new { id = forumTopicId, pageNumber = (x), slug = forumTopicSlug }, new { title = String.Format(localizationService.GetResource("Pager.PageLinkTitle"), x.ToString()) }));

                        if (x < totalPages)
                        {
                            links.Append(", ");
                        }
                    }
                }

                // Inserts the topic page links into the localized string ([Go to page: {0}])
                return new HtmlString(String.Format(localizationService.GetResource("Forum.Topics.GotoPostPager"), links));
            }
            return new HtmlString(string.Empty);
        }        
    }
}

