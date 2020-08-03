using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Commands.Models.Products
{
    public class SendProductEmailAFriendMessageCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public Product Product { get; set; }
        public ProductEmailAFriendModel Model { get; set; }
    }
}
