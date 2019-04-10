using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IShoppingCartViewModelService
    {
        Task<PictureModel> PrepareCartItemPicture(Product product, string attributesXml, int pictureSize, bool showDefaultPicture, string productName);
        Task PrepareShoppingCart(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false);
        Task PrepareWishlist(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true);

        Task<MiniShoppingCartModel> PrepareMiniShoppingCart();

        Task<EstimateShippingModel> PrepareEstimateShipping(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true);

        Task<OrderTotalsModel> PrepareOrderTotals(IList<ShoppingCartItem> cart, bool isEditable);

        Task<AddToCartModel> PrepareAddToCartModel(Product product, Customer customer, int quantity, decimal customerEnteredPrice, string attributesXml, ShoppingCartType cartType, DateTime? startDate, DateTime? endDate, string reservationId, string parameter, string duration);

        Task ParseAndSaveCheckoutAttributes(IList<ShoppingCartItem> cart, IFormCollection form);

        Task<string> ParseProductAttributes(Product product, IFormCollection form);

        void ParseReservationDates(Product product, IFormCollection form,
            out DateTime? startDate, out DateTime? endDate);

        Task<EstimateShippingResultModel> PrepareEstimateShippingResult(IList<ShoppingCartItem> cart, string countryId, string stateProvinceId, string zipPostalCode);
    }
}
