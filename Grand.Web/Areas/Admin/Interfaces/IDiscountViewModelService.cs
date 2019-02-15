using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Vendors;
using Grand.Services.Discounts;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Discounts;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IDiscountViewModelService
    {
        DiscountListModel PrepareDiscountListModel();
        (IEnumerable<DiscountModel> discountModel, int totalCount) PrepareDiscountModel(DiscountListModel model, int pageIndex, int pageSize);
        void PrepareDiscountModel(DiscountModel model, Discount discount);
        Discount InsertDiscountModel(DiscountModel model);
        Discount UpdateDiscountModel(Discount discount, DiscountModel model);
        void DeleteDiscount(Discount discount);
        void InsertCouponCode(string discountId, string couponCode);
        string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, string discountRequirementId);
        void DeleteDiscountRequirement(DiscountRequirement discountRequirement, Discount discount);
        DiscountModel.AddProductToDiscountModel PrepareProductToDiscountModel();
        (IList<ProductModel> products, int totalCount) PrepareProductModel(DiscountModel.AddProductToDiscountModel model, int pageIndex, int pageSize);
        void InsertProductToDiscountModel(DiscountModel.AddProductToDiscountModel model);
        void DeleteProduct(Discount discount, Product product);
        void DeleteCategory(Discount discount, Category category);
        void DeleteVendor(Discount discount, Vendor vendor);
        void DeleteManufacturer(Discount discount, Manufacturer manufacturer);
        void DeleteStore(Discount discount, Store store);
        void InsertCategoryToDiscountModel(DiscountModel.AddCategoryToDiscountModel model);
        void InsertManufacturerToDiscountModel(DiscountModel.AddManufacturerToDiscountModel model);
        void InsertVendorToDiscountModel(DiscountModel.AddVendorToDiscountModel model);
        void InsertStoreToDiscountModel(DiscountModel.AddStoreToDiscountModel model);
        (IEnumerable<DiscountModel.DiscountUsageHistoryModel> usageHistoryModels, int totalCount) PrepareDiscountUsageHistoryModel(Discount discount, int pageIndex, int pageSize);

    }
}
