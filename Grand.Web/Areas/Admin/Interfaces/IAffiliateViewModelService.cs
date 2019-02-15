using Grand.Core.Domain.Affiliates;
using Grand.Web.Areas.Admin.Models.Affiliates;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IAffiliateViewModelService
    {
        void PrepareAffiliateModel(AffiliateModel model, Affiliate affiliate, bool excludeProperties,
            bool prepareEntireAddressModel = true);
        (IEnumerable<AffiliateModel> affiliateModels, int totalCount) PrepareAffiliateModelList(AffiliateListModel model, int pageIndex, int pageSize);
        Affiliate InsertAffiliateModel(AffiliateModel model);
        Affiliate UpdateAffiliateModel(AffiliateModel model, Affiliate affiliate);
        (IEnumerable<AffiliateModel.AffiliatedOrderModel> affiliateOrderModels, int totalCount) PrepareAffiliatedOrderList(Affiliate affiliate, AffiliatedOrderListModel model, int pageIndex, int pageSize);
        (IEnumerable<AffiliateModel.AffiliatedCustomerModel> affiliateCustomerModels, int totalCount) PrepareAffiliatedCustomerList(Affiliate affiliate, int pageIndex, int pageSize);
    }
}
