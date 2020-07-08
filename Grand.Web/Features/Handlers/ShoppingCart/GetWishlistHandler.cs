using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetWishlistHandler : IRequestHandler<GetWishlist, WishlistModel>
    {
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;

        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;

        public GetWishlistHandler(
            IPermissionService permissionService, 
            IShoppingCartService shoppingCartService, 
            IProductService productService, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeFormatter productAttributeFormatter, 
            ILocalizationService localizationService, 
            ITaxService taxService, 
            IPriceCalculationService priceCalculationService, 
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            ICacheManager cacheManager, 
            IPictureService pictureService, 
            ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings, 
            MediaSettings mediaSettings)
        {
            _permissionService = permissionService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _localizationService = localizationService;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<WishlistModel> Handle(GetWishlist request, CancellationToken cancellationToken)
        {
            var model = new WishlistModel();
            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = request.IsEditable;
            model.DisplayAddToCart = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

            if (!request.Cart.Any())
                return model;

            #region Simple properties

            model.CustomerGuid = request.Customer.CustomerGuid;
            model.CustomerFullname = request.Customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnWishList;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarnings(request.Cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Cart items

            foreach (var sci in request.Cart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                var cartItemModel = new WishlistModel.ShoppingCartItemModel {
                    Id = sci.Id,
                    Sku = product.FormatSku(sci.AttributesXml, _productAttributeParser),
                    ProductId = product.Id,
                    ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                    ProductSeName = product.GetSeName(request.Language.Id),
                    Quantity = sci.Quantity,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                };

                //allow editing?
                //1. setting enabled?
                //2. simple product?
                //3. has attribute or gift card?
                //4. visible individually?
                cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing && product.ProductType == ProductType.SimpleProduct && (!String.IsNullOrEmpty(cartItemModel.AttributeInfo) || product.IsGiftCard) && product.VisibleIndividually;

                //allowed quantities
                var allowedQuantities = product.ParseAllowedQuantities();
                foreach (var qty in allowedQuantities)
                {
                    cartItemModel.AllowedQuantities.Add(new SelectListItem {
                        Text = qty.ToString(),
                        Value = qty.ToString(),
                        Selected = sci.Quantity == qty
                    });
                }


                //recurring info
                if (product.IsRecurring)
                    cartItemModel.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, request.Language.Id));

                //unit prices
                if (product.CallForPrice)
                {
                    cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    var productprice = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                    decimal taxRate = productprice.taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = productprice.productprice;
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, request.Currency);
                    cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                }
                //subtotal, discount
                if (product.CallForPrice)
                {
                    cartItemModel.SubTotal = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    var subtotal = await _priceCalculationService.GetSubTotal(sci, true);
                    decimal shoppingCartItemDiscountBase = subtotal.discountAmount;
                    List<AppliedDiscount> scDiscounts = subtotal.appliedDiscounts;
                    var productprices = await _taxService.GetProductPrice(product, subtotal.subTotal);
                    decimal taxRate = productprices.taxRate;
                    decimal shoppingCartItemSubTotalWithDiscountBase = productprices.productprice;

                    decimal shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, request.Currency);
                    cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                    //display an applied discount amount
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        shoppingCartItemDiscountBase = (await _taxService.GetProductPrice(product, shoppingCartItemDiscountBase)).productprice;
                        if (shoppingCartItemDiscountBase > decimal.Zero)
                        {
                            decimal shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, request.Currency);
                            cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                        }
                    }
                }

                //picture
                if (_shoppingCartSettings.ShowProductImagesOnWishList)
                {
                    cartItemModel.Picture = await PrepareCartItemPicture(request, product, sci.AttributesXml);
                }

                //item warnings
                var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarnings(request.Customer, sci, product, false);
                foreach (var warning in itemWarnings)
                    cartItemModel.Warnings.Add(warning);

                model.Items.Add(cartItemModel);
            }
            
            #endregion
            
            return model;
        }

        private async Task<PictureModel> PrepareCartItemPicture(GetWishlist request, 
            Product product, string attributesXml)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConst.CART_PICTURE_MODEL_KEY, product.Id, _mediaSettings.CartThumbPictureSize, 
                true, request.Language.Id, request.Store.Id);

            var model = await _cacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
                return new PictureModel {
                    Id = sciPicture?.Id,
                    ImageUrl = await _pictureService.GetPictureUrl(sciPicture, _mediaSettings.CartThumbPictureSize, true),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), product.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), product.Name),
                };
            });

            return model;
        }

    }
}
