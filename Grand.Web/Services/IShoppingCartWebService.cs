using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Grand.Core.Domain.Customers;

namespace Grand.Web.Services
{
    public partial interface IShoppingCartWebService
    {
        PictureModel PrepareCartItemPicture(Product product, string attributesXml, int pictureSize, bool showDefaultPicture, string productName);
        void PrepareShoppingCart(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false);
        void PrepareWishlist(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true);

        MiniShoppingCartModel PrepareMiniShoppingCart();

        EstimateShippingModel PrepareEstimateShipping(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true);

        OrderTotalsModel PrepareOrderTotals(IList<ShoppingCartItem> cart, bool isEditable);

        AddToCartModel PrepareAddToCartModel(Product product, Customer customer, int quantity, decimal customerEnteredPrice, string attributesXml, ShoppingCartType cartType, DateTime? startDate, DateTime? endDate, string reservationId, string parameter, string duration);

        void ParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, IFormCollection form);

        string ParseProductAttributes(Product product, IFormCollection form);

        void ParseReservationDates(Product product, IFormCollection form,
            out DateTime? startDate, out DateTime? endDate);

        EstimateShippingResultModel PrepareEstimateShippingResult(List<ShoppingCartItem> cart, string countryId, string stateProvinceId, string zipPostalCode);

    }
}