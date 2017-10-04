using Grand.Core.Domain.Forums;
using Grand.Web.Models.Boards;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Services
{
    public partial interface IBoardsWebService
    {
        BoardsIndexModel PrepareBoardsIndex();
        ActiveDiscussionsModel PrepareActiveDiscussions();
        ActiveDiscussionsModel PrepareActiveDiscussions(string forumId = "", int pageNumber = 1);
        ForumTopicRowModel PrepareForumTopicRow(ForumTopic topic);
        ForumPageModel PrepareForumPage(Forum forum, int pageNumber);
        ForumRowModel PrepareForumRow(Forum forum);
        ForumGroupModel PrepareForumGroup(ForumGroup forumGroup);
        IEnumerable<SelectListItem> ForumTopicTypesList();
        IEnumerable<SelectListItem> ForumGroupsForumsList();
        ForumTopicPageModel PrepareForumTopicPage(ForumTopic forumTopic, int pageNumber);
        TopicMoveModel PrepareTopicMove(ForumTopic forumTopic);
        EditForumTopicModel PrepareEditForumTopic(Forum forum);
        EditForumPostModel PrepareEditForumPost(Forum forum, ForumTopic forumTopic, string quote);
        LastPostModel PrepareLastPost(ForumPost post, bool showTopic);
        ForumBreadcrumbModel PrepareForumBreadcrumb(string forumGroupId, string forumId, string forumTopicId);
        CustomerForumSubscriptionsModel PrepareCustomerForumSubscriptions(int pageIndex);
        SearchModel PrepareSearch(string searchterms, bool? adv, string forumId,
            string within, string limitDays, int pageNumber = 1);
    }
}