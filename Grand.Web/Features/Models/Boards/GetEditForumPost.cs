using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetEditForumPost : IRequest<EditForumPostModel>
    {
        public Customer Customer { get; set; }
        public Forum Forum { get; set; }
        public ForumTopic ForumTopic { get; set; }
        public string Quote { get; set; }
    }
}
