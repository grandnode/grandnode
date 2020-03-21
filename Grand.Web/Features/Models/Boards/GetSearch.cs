using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetSearch : IRequest<SearchModel>
    {
        public string Searchterms { get; set; }
        public bool? Adv { get; set; }
        public string ForumId { get; set; }
        public string Within { get; set; }
        public string LimitDays { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
