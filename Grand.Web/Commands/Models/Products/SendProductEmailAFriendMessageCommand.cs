using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Commands.Models.Products
{
    public class SendProductEmailAFriendMessageCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Product Product { get; set; }
        public ProductEmailAFriendModel Model { get; set; }
    }
}
