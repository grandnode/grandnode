using Grand.Core.Domain.Catalog;
using MediatR;

namespace Grand.Services.Events.Web
{
    public class UpdateProductOnCartEvent : INotification
    {
        private readonly Product _product;

        public UpdateProductOnCartEvent(Product product)
        {
            _product = product;
        }

        public Product Product { get { return _product; } }

    }
}
