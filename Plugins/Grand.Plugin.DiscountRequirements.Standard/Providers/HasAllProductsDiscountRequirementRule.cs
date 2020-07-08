using Grand.Domain.Orders;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Orders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.DiscountRequirements.HasAllProducts
{
    public partial class HasAllProductsDiscountRequirementRule : IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public HasAllProductsDiscountRequirementRule(ISettingService settingService, ShoppingCartSettings shoppingCartSettings)
        {
            _settingService = settingService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            var restrictedProductIds = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirements.Standard.RestrictedProductIds-{0}-{1}", request.DiscountId, request.DiscountRequirementId));

            if (String.IsNullOrWhiteSpace(restrictedProductIds))
            {
                //valid
                result.IsValid = true;
                return result;
            }

            if (request.Customer == null)
                return result;


            //we support three ways of specifying products:
            //1. The comma-separated list of product identifiers (e.g. 77, 123, 156).
            //2. The comma-separated list of product identifiers with quantities.
            //      {Product ID}:{Quantity}. For example, 77:1, 123:2, 156:3
            //3. The comma-separated list of product identifiers with quantity range.
            //      {Product ID}:{Min quantity}-{Max quantity}. For example, 77:1-3, 123:2-5, 156:3-8
            var restrictedProducts = restrictedProductIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            if (restrictedProducts.Count == 0)
                return result;

            //group products in the cart by product ID
            //it could be the same product with distinct product attributes
            //that's why we get the total quantity of this product
            var cartQuery = from sci in request.Customer.ShoppingCartItems.LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, request.Store.Id)
                            where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
                            group sci by sci.ProductId into g
                            select new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) };
            var cart = cartQuery.ToList();

            bool allFound = true;
            foreach (var restrictedProduct in restrictedProducts)
            {
                if (String.IsNullOrWhiteSpace(restrictedProduct))
                    continue;

                bool found1 = false;
                foreach (var sci in cart)
                {
                    if (restrictedProduct.Contains(":"))
                    {
                        if (restrictedProduct.Contains("-"))
                        {
                            //the third way (the quantity rage specified)
                            //{Product ID}:{Min quantity}-{Max quantity}. For example, 77:1-3, 123:2-5, 156:3-8
                            string restrictedProductId = restrictedProduct.Split(new[] { ':' })[0];
                            int quantityMin;
                            if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[0], out quantityMin))
                                //parsing error; exit;
                                return result;
                            int quantityMax;
                            if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[1], out quantityMax))
                                //parsing error; exit;
                                return result;

                            if (sci.ProductId == restrictedProductId && quantityMin <= sci.TotalQuantity && sci.TotalQuantity <= quantityMax)
                            {
                                found1 = true;
                                break;
                            }
                        }
                        else
                        {
                            //the second way (the quantity specified)
                            //{Product ID}:{Quantity}. For example, 77:1, 123:2, 156:3
                            string restrictedProductId = restrictedProduct.Split(new[] { ':' })[0];
                            int quantity;
                            if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[1], out quantity))
                                //parsing error; exit;
                                return result;

                            if (sci.ProductId == restrictedProductId && sci.TotalQuantity == quantity)
                            {
                                found1 = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        //the first way (the quantity is not specified)
                        if (sci.ProductId == restrictedProduct)
                        {
                            found1 = true;
                            break;
                        }
                    }
                }

                if (!found1)
                {
                    allFound = false;
                    break;
                }
            }

            if (allFound)
            {
                //valid
                result.IsValid = true;
                return result;
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(string discountId, string discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Admin/HasAllProducts/Configure/?discountId=" + discountId;
            if (!String.IsNullOrEmpty(discountRequirementId))
                result += string.Format("&discountRequirementId={0}", discountRequirementId);
            return result;
        }

        public string FriendlyName => "Customer has all of these products in";

        public string SystemName => "DiscountRequirements.HasAllProducts";
    }
}