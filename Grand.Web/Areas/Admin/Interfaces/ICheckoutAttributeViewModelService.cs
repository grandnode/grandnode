using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICheckoutAttributeViewModelService
    {
        Task<IEnumerable<CheckoutAttributeModel>> PrepareCheckoutAttributeListModel();
        Task<IEnumerable<CheckoutAttributeValueModel>> PrepareCheckoutAttributeValuesModel(string checkoutAttributeId);
        Task<CheckoutAttributeModel> PrepareCheckoutAttributeModel();
        Task<CheckoutAttributeValueModel> PrepareCheckoutAttributeValueModel(string checkoutAttributeId);
        Task<CheckoutAttributeValueModel> PrepareCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue);
        Task PrepareTaxCategories(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties);
        Task PrepareConditionAttributes(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute);
        Task<CheckoutAttribute> InsertCheckoutAttributeModel(CheckoutAttributeModel model);
        Task<CheckoutAttribute> UpdateCheckoutAttributeModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model);
        Task<CheckoutAttributeValue> InsertCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValueModel model);
        Task<CheckoutAttributeValue> UpdateCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model);
    }
}
