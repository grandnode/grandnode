using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Services.Notifications.Catalog;
using MediatR;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class UpdateProductOnCartEventHandler : INotificationHandler<UpdateProductOnCartEvent>
    {
        private readonly IRepository<Customer> _customerRepository;

        public UpdateProductOnCartEventHandler(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task Handle(UpdateProductOnCartEvent notification, CancellationToken cancellationToken)
        {
            var builderCustomer = Builders<Customer>.Filter;
            var filterCustomer = builderCustomer.ElemMatch(x => x.ShoppingCartItems, y => y.ProductId == notification.Product.Id);
            await _customerRepository.Collection.Find(filterCustomer).ForEachAsync(async (cs) =>
            {
                foreach (var item in cs.ShoppingCartItems.Where(x => x.ProductId == notification.Product.Id))
                {
                    var updateCustomer = Builders<Customer>.Update
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).AdditionalShippingChargeProduct, notification.Product.AdditionalShippingCharge)
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).IsFreeShipping, notification.Product.IsFreeShipping)
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).IsGiftCard, notification.Product.IsGiftCard)
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).IsShipEnabled, notification.Product.IsShipEnabled)
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).IsTaxExempt, notification.Product.IsTaxExempt)
                        .Set(x => x.ShoppingCartItems.ElementAt(-1).IsRecurring, notification.Product.IsRecurring);

                    var _builderCustomer = Builders<Customer>.Filter;
                    var _filterCustomer = _builderCustomer.ElemMatch(x => x.ShoppingCartItems, y => y.Id == item.Id);
                    await _customerRepository.Collection.UpdateManyAsync(_filterCustomer, updateCustomer);
                }
            }
            );
        }
    }
}
