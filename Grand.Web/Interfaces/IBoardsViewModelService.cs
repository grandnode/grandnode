using Grand.Core.Domain.Forums;
using Grand.Web.Models.Boards;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IBoardsViewModelService
    {
        Task<BoardsIndexModel> PrepareBoardsIndex();
        Task<ActiveDiscussionsModel> PrepareActiveDiscussions();
        Task<ActiveDiscussionsModel> PrepareActiveDiscussions(string forumId = "", int pageNumber = 1);
        Task<ForumTopicRowModel> PrepareForumTopicRow(ForumTopic topic);
        Task<ForumPageModel> PrepareForumPage(Forum forum, int pageNumber);
        ForumRowModel PrepareForumRow(Forum forum);
        Task<ForumGroupModel> PrepareForumGroup(ForumGroup forumGroup);
        IEnumerable<SelectListItem> ForumTopicTypesList();
        Task<IEnumerable<SelectListItem>> ForumGroupsForumsList();
        Task<ForumTopicPageModel> PrepareForumTopicPage(ForumTopic forumTopic, int pageNumber);
        Task<TopicMoveModel> PrepareTopicMove(ForumTopic forumTopic);
        EditForumTopicModel PrepareEditForumTopic(Forum forum);
        Task<EditForumPostModel> PrepareEditForumPost(Forum forum, ForumTopic forumTopic, string quote);
        Task<LastPostModel> PrepareLastPost(string forumPostId, bool showTopic);
        Task<ForumBreadcrumbModel> PrepareForumBreadcrumb(string forumGroupId, string forumId, string forumTopicId);
        Task<CustomerForumSubscriptionsModel> PrepareCustomerForumSubscriptions(int pageIndex);
        Task<SearchModel> PrepareSearch(string searchterms, bool? adv, string forumId,
            string within, string limitDays, int pageNumber = 1);
    }
}