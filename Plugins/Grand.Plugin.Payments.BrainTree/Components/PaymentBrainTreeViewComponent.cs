using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Braintree;
using Grand.Core;
using Grand.Domain.Orders;
using Grand.Plugin.Payments.BrainTree;
using Grand.Plugin.Payments.BrainTree.Models;
using Grand.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Plugin.Payments.BrainTree.Components
{
    [ViewComponent(Name = "PaymentBrainTree")]
    public class PaymentBrainTreeViewComponent : ViewComponent
    {
        private readonly BrainTreePaymentSettings _brainTreePaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public PaymentBrainTreeViewComponent(BrainTreePaymentSettings brainTreePaymentSettings, 
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _brainTreePaymentSettings = brainTreePaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel();

            if (_brainTreePaymentSettings.Use3DS)
            {
                var cart = _shoppingCartService.GetShoppingCart(_storeContext.CurrentStore.Id, ShoppingCartType.ShoppingCart);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                //get settings
                var useSandBox = _brainTreePaymentSettings.UseSandBox;
                var merchantId = _brainTreePaymentSettings.MerchantId;
                var publicKey = _brainTreePaymentSettings.PublicKey;
                var privateKey = _brainTreePaymentSettings.PrivateKey;

                var gateway = new BraintreeGateway {
                    Environment = useSandBox ? Braintree.Environment.SANDBOX : Braintree.Environment.PRODUCTION,
                    MerchantId = merchantId,
                    PublicKey = publicKey,
                    PrivateKey = privateKey
                };

                ViewBag.ClientToken = gateway.ClientToken.Generate();
                ViewBag.OrderTotal = (await _orderTotalCalculationService.GetShoppingCartTotal(cart)).shoppingCartTotal;

                return View("~/Plugins/Payments.BrainTree/Views/PaymentInfo_3DS.cshtml", model);
            }

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                var text = (i < 10) ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method == WebRequestMethods.Http.Get)
                return View("~/Plugins/Payments.BrainTree/Views/PaymentInfo.cshtml", model);

            var form = await HttpContext.Request.ReadFormAsync();

            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];

            var selectedMonth = model.ExpireMonths
                .FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears
                .FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            return View("~/Plugins/Payments.BrainTree/Views/PaymentInfo.cshtml", model);
        }
    }
}