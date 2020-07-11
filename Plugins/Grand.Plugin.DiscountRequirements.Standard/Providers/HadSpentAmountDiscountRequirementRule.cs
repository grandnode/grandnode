using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Orders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount
{
    public partial class HadSpentAmountDiscountRequirementRule : IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;

        public HadSpentAmountDiscountRequirementRule(ISettingService settingService, IOrderService orderService, ILocalizationService localizationService)
        {
            _settingService = settingService;
            _orderService = orderService;
            _localizationService = localizationService;
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

            var spentAmountRequirement = _settingService.GetSettingByKey<decimal>(string.Format("DiscountRequirements.Standard.HadSpentAmount-{0}-{1}", request.DiscountId, request.DiscountRequirementId));

            if (spentAmountRequirement == decimal.Zero)
            {
                //valid
                result.IsValid = true;
                return result;
            }

            if (request.Customer == null || request.Customer.IsGuest())
                return result;

            var orders = await _orderService.SearchOrders(storeId: request.Store.Id,
                customerId: request.Customer.Id,
                os: OrderStatus.Complete);
            decimal spentAmount = orders.Sum(o => o.OrderTotal);
            if (spentAmount > spentAmountRequirement)
            {
                result.IsValid = true;
            }
            else
            {
                result.UserError = _localizationService.GetResource("Plugins.DiscountRequirements.Standard.HadSpentAmount.NotEnough");
            }
            return result;
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
            string result = "Admin/HadSpentAmount/Configure/?discountId=" + discountId;
            if (!String.IsNullOrEmpty(discountRequirementId))
                result += string.Format("&discountRequirementId={0}", discountRequirementId);
            return result;
        }

        public string FriendlyName => "Customer had spent x.xx amount";

        public string SystemName => "DiscountRequirements.Standard.HadSpentAmount";
    }
}