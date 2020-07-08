using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Web.Areas.Admin.Models.ShoppingCart;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.CurrentCarts)]
    public partial class ShoppingCartController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        #endregion

        #region Constructors

        public ShoppingCartController(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IStoreService storeService,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _storeService = storeService;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _localizationService = localizationService;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
        }

        #endregion

        #region Methods

        //shopping carts
        public IActionResult CurrentCarts() => View();

        [HttpPost]
        public async Task<IActionResult> CurrentCarts(DataSourceRequest command)
        {
            var customers = await _customerService.GetAllCustomers(
                loadOnlyWithShoppingCart: true,
                sct: ShoppingCartType.ShoppingCart,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers.Select(x => new ShoppingCartModel
                {
                    CustomerId = x.Id,
                    CustomerEmail = x.IsRegistered() ? x.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                    TotalItems = x.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).Sum(y => y.Quantity)
                }),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetCartDetails(string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            var items = new List<ShoppingCartItemModel>();
            foreach (var sci in cart)
            {
                var store = await _storeService.GetStoreById(sci.StoreId);
                var product = await _productService.GetProductById(sci.ProductId);
                var sciModel = new ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Store = store != null ? store.Shortcut : "Unknown",
                    ProductId = sci.ProductId,
                    Quantity = sci.Quantity,
                    ProductName = product.Name,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml, customer),
                    UnitPrice = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice)).productprice),
                    Total = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetSubTotal(sci)).subTotal)).productprice),
                    UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                };
                items.Add(sciModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = cart.Count
            };
            return Json(gridModel);
        }

        //wishlists
        [HttpPost]
        public async Task<IActionResult> CurrentWishlists(DataSourceRequest command)
        {
            var customers = await _customerService.GetAllCustomers(
                loadOnlyWithShoppingCart: true,
                sct: ShoppingCartType.Wishlist,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = customers.Select(x => new ShoppingCartModel
                {
                    CustomerId = x.Id,
                    CustomerEmail = x.IsRegistered() ? x.Email : _localizationService.GetResource("Admin.Customers.Guest"),
                    TotalItems = x.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).Sum(y => y.Quantity)
                }),
                Total = customers.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetWishlistDetails(string customerId)
        {
            var customer = await _customerService.GetCustomerById(customerId);
            var cart = customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).ToList();
            var items = new List<ShoppingCartItemModel>();
            foreach (var sci in cart)
            {
                var store = await _storeService.GetStoreById(sci.StoreId);
                var product = await _productService.GetProductById(sci.ProductId);
                var sciModel = new ShoppingCartItemModel
                {
                    Id = sci.Id,
                    Store = store != null ? store.Shortcut : "Unknown",
                    ProductId = sci.ProductId,
                    Quantity = sci.Quantity,
                    ProductName = product.Name,
                    AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml, customer),
                    UnitPrice = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice)).productprice),
                    Total = _priceFormatter.FormatPrice((await _taxService.GetProductPrice(product, (await _priceCalculationService.GetSubTotal(sci)).subTotal)).productprice),
                    UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.UpdatedOnUtc, DateTimeKind.Utc)
                };
                items.Add(sciModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = cart.Count
            };

            return Json(gridModel);
        }

        #endregion
    }
}
