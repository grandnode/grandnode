using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Boards
{
    public class GetTopicTypesList : IRequest<IEnumerable<SelectListItem>>
    {
    }
}
