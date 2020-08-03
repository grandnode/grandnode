using Grand.Domain.Forums;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Boards;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetTopicTypesListHandler : IRequestHandler<GetTopicTypesList, IEnumerable<SelectListItem>>
    {
        private readonly ILocalizationService _localizationService;

        public GetTopicTypesListHandler(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<IEnumerable<SelectListItem>> Handle(GetTopicTypesList request, CancellationToken cancellationToken)
        {
            var list = new List<SelectListItem>();

            list.Add(new SelectListItem {
                Text = _localizationService.GetResource("Forum.Normal"),
                Value = ((int)ForumTopicType.Normal).ToString()
            });

            list.Add(new SelectListItem {
                Text = _localizationService.GetResource("Forum.Sticky"),
                Value = ((int)ForumTopicType.Sticky).ToString()
            });

            list.Add(new SelectListItem {
                Text = _localizationService.GetResource("Forum.Announcement"),
                Value = ((int)ForumTopicType.Announcement).ToString()
            });
            return await Task.FromResult(list);
        }
    }
}
