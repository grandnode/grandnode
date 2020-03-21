using Grand.Services.Forums;
using Grand.Web.Features.Models.Boards;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetGroupsForumsListHandler : IRequestHandler<GetGroupsForumsList, IEnumerable<SelectListItem>>
    {
        private readonly IForumService _forumService;

        public GetGroupsForumsListHandler(IForumService forumService)
        {
            _forumService = forumService;
        }

        public async Task<IEnumerable<SelectListItem>> Handle(GetGroupsForumsList request, CancellationToken cancellationToken)
        {
            var forumsList = new List<SelectListItem>();
            var separator = "--";
            var forumGroups = await _forumService.GetAllForumGroups();

            foreach (var fg in forumGroups)
            {
                // Add the forum group with Value of 0 so it won't be used as a target forum
                forumsList.Add(new SelectListItem { Text = fg.Name, Value = "" });

                var forums = await _forumService.GetAllForumsByGroupId(fg.Id);
                foreach (var f in forums)
                {
                    forumsList.Add(new SelectListItem { Text = string.Format("{0}{1}", separator, f.Name), Value = f.Id.ToString() });
                }
            }
            return forumsList;
        }
    }
}
