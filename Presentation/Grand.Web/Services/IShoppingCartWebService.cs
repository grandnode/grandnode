using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Orders;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Grand.Web.Services
{
    public partial interface IShoppingCartWebService
    {
        PictureModel PrepareCartItemPicture(ShoppingCartItem sci,
            int pictureSize, bool showDefaultPicture, string productName);
        void PrepareShoppingCart(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false);
        void PrepareWishlist(WishlistModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true);

        MiniShoppingCartModel PrepareMiniShoppingCart();

        OrderTotalsModel PrepareOrderTotals(IList<ShoppingCartItem> cart, bool isEditable);

        void ParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, FormCollection form);

        string ParseProductAttributes(Product product, FormCollection form);

        void ParseRentalDates(Product product, FormCollection form,
            out DateTime? startDate, out DateTime? endDate);

        EstimateShippingResultModel PrepareEstimateShippingResult(List<ShoppingCartItem> cart, string countryId, string stateProvinceId, string zipPostalCode);

    }
}