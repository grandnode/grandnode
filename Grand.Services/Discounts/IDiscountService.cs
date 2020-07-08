using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Discounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Discount service interface
    /// </summary>
    public partial interface IDiscountService
    {
        /// <summary>
        /// Delete discount
        /// </summary>
        /// <param name="discount">Discount</param>
        Task DeleteDiscount(Discount discount);

        /// <summary>
        /// Gets a discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        Task<Discount> GetDiscountById(string discountId);

        /// <summary>
        /// Gets all discounts
        /// </summary>
        /// <param name="discountType">Discount type; null to load all discount</param>
        /// <param name="couponCode">Coupon code to find (exact match)</param>
        /// <param name="discountName">Discount name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discounts</returns>
        Task<IList<Discount>> GetAllDiscounts(DiscountType? discountType, string storeId = "",
            string couponCode = "", string discountName = "", bool showHidden = false);

        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">Discount</param>
        Task InsertDiscount(Discount discount);

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">Discount</param>
        Task UpdateDiscount(Discount discount);

        /// <summary>
        /// Delete discount requirement
        /// </summary>
        /// <param name="discountRequirement">Discount requirement</param>
        Task DeleteDiscountRequirement(DiscountRequirement discountRequirement);

        /// <summary>
        /// Load discount plugin by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Discount plugin</returns>
        IDiscount LoadDiscountPluginBySystemName(string systemName);

        /// <summary>
        /// Load all discount plugins
        /// </summary>
        /// <returns>Discount requirement rules</returns>
        IList<IDiscount> LoadAllDiscountPlugins();


        /// <summary>
        /// Get discount by coupon code
        /// </summary>
        /// <param name="couponCode">CouponCode</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discount</returns>
        Task<Discount> GetDiscountByCouponCode(string couponCode, bool showHidden = false);

        /// <summary>
        /// Exist coupon code in discount
        /// </summary>
        /// <param name="couponCode"></param>
        /// <param name="discountId"></param>
        /// <returns></returns>
        Task<bool> ExistsCodeInDiscount(string couponCode, string discountId, bool? used);

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discount validation result</returns>
        Task<DiscountValidationResult> ValidateDiscount(Discount discount, Customer customer);

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodeToValidate">Coupon code to validate</param>
        /// <returns>Discount validation result</returns>
        Task<DiscountValidationResult> ValidateDiscount(Discount discount, Customer customer, string couponCodeToValidate);

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>Discount validation result</returns>
        Task<DiscountValidationResult> ValidateDiscount(Discount discount, Customer customer, string[] couponCodesToValidate);


        /// <summary>
        /// Gets a discount usage history record
        /// </summary>
        /// <param name="discountUsageHistoryId">Discount usage history record identifier</param>
        /// <returns>Discount usage history</returns>
        Task<DiscountUsageHistory> GetDiscountUsageHistoryById(string discountUsageHistoryId);

        /// <summary>
        /// Gets all discount usage history records
        /// </summary>
        /// <param name="discountId">Discount identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Discount usage history records</returns>
        Task<IPagedList<DiscountUsageHistory>> GetAllDiscountUsageHistory(string discountId = "",
            string customerId = "", string orderId = "", bool? canceled = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Insert discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        Task InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory);

        /// <summary>
        /// Update discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        Task UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory);

        /// <summary>
        /// Delete discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        Task DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory);

        /// <summary>
        /// Get all coupon codes for discount
        /// </summary>
        /// <param name="discountId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<DiscountCoupon>> GetAllCouponCodesByDiscountId(string discountId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Get discount code by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DiscountCoupon> GetDiscountCodeById(string id);

        /// <summary>
        /// Get discount code by discount code
        /// </summary>
        /// <param name="couponCode"></param>
        /// <returns></returns>
        Task<DiscountCoupon> GetDiscountCodeByCode(string couponCode);

        /// <summary>
        /// Delete discount code
        /// </summary>
        /// <param name="coupon"></param>
        Task DeleteDiscountCoupon(DiscountCoupon coupon);

        /// <summary>
        /// Update discount code - set as used or not
        /// </summary>
        /// <param name="couponCode"></param>
        /// <param name="discountId"></param>
        /// <param name="used"></param>
        Task DiscountCouponSetAsUsed(string couponCode, bool used);

        /// <summary>
        /// Cancel discount if order was canceled or deleted
        /// </summary>
        /// <param name="orderId"></param>
        Task CancelDiscount(string orderId);

        /// <summary>
        /// Insert discount code
        /// </summary>
        /// <param name="coupon"></param>
        Task InsertDiscountCoupon(DiscountCoupon coupon);

        /// <summary>
        /// Get discount amount from plugin
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<decimal> GetDiscountAmount(Discount discount, Customer customer, Product product, decimal amount);

        /// <summary>
        /// Get preferred discount
        /// </summary>
        /// <param name="discounts"></param>
        /// <param name="amount"></param>
        /// <param name="customer"></param>
        /// <param name="product"></param>
        /// <param name="discountAmount"></param>
        /// <returns></returns>
        Task<(List<AppliedDiscount> appliedDiscount, decimal discountAmount)> GetPreferredDiscount(IList<AppliedDiscount> discounts,
            Customer customer, Product product, decimal amount);

        /// <summary>
        /// Get preferred discount
        /// </summary>
        /// <param name="discounts"></param>
        /// <param name="amount"></param>
        /// <param name="customer"></param>
        /// <param name="discountAmount"></param>
        /// <returns></returns>
        Task<(List<AppliedDiscount> appliedDiscount, decimal discountAmount)> GetPreferredDiscount(IList<AppliedDiscount> discounts,
            Customer customer, decimal amount);

        /// <summary>
        /// GetDiscountAmountProvider
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="customer"></param>
        /// <param name="product"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<decimal> GetDiscountAmountProvider(Discount discount, Customer customer, Product product, decimal amount);

        /// <summary>
        /// Load discount amount provider by systemName
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        IDiscountAmountProvider LoadDiscountAmountProviderBySystemName(string systemName);

        /// <summary>
        /// Load discount amount providers
        /// </summary>
        /// <returns></returns>
        IList<IDiscountAmountProvider> LoadDiscountAmountProviders();
    }
}
