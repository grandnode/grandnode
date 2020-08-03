using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetForumBreadcrumbHandler : IRequestHandler<GetForumBreadcrumb, ForumBreadcrumbModel>
    {
        private readonly IForumService _forumService;

        public GetForumBreadcrumbHandler(IForumService forumService)
        {
            _forumService = forumService;
        }

        public async Task<ForumBreadcrumbModel> Handle(GetForumBreadcrumb request, CancellationToken cancellationToken)
        {
            var model = new ForumBreadcrumbModel();

            ForumTopic forumTopic = null;
            if (!String.IsNullOrEmpty(request.ForumTopicId))
            {
                forumTopic = await _forumService.GetTopicById(request.ForumTopicId);
                if (forumTopic != null)
                {
                    model.ForumTopicId = forumTopic.Id;
                    model.ForumTopicSubject = forumTopic.Subject;
                    model.ForumTopicSeName = forumTopic.GetSeName();
                }
            }

            Forum forum = await _forumService.GetForumById(forumTopic != null ? forumTopic.ForumId : (!String.IsNullOrEmpty(request.ForumId) ? request.ForumId : ""));
            if (forum != null)
            {
                model.ForumId = forum.Id;
                model.ForumName = forum.Name;
                model.ForumSeName = forum.GetSeName();
            }

            var forumGroup = await _forumService.GetForumGroupById(forum != null ? forum.ForumGroupId : (!String.IsNullOrEmpty(request.ForumGroupId) ? request.ForumGroupId : ""));
            if (forumGroup != null)
            {
                model.ForumGroupId = forumGroup.Id;
                model.ForumGroupName = forumGroup.Name;
                model.ForumGroupSeName = forumGroup.GetSeName();
            }

            return model;
        }
    }
}
