using Grand.Core.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface ICheckoutAttributeViewModelService
    {
        IEnumerable<CheckoutAttributeModel> PrepareCheckoutAttributeListModel();
        IEnumerable<CheckoutAttributeValueModel> PrepareCheckoutAttributeValuesModel(string checkoutAttributeId);
        CheckoutAttributeModel PrepareCheckoutAttributeModel();
        CheckoutAttributeValueModel PrepareCheckoutAttributeValueModel(string checkoutAttributeId);
        CheckoutAttributeValueModel PrepareCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue);
        void PrepareTaxCategories(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute, bool excludeProperties);
        void PrepareConditionAttributes(CheckoutAttributeModel model, CheckoutAttribute checkoutAttribute);
        CheckoutAttribute InsertCheckoutAttributeModel(CheckoutAttributeModel model);
        CheckoutAttribute UpdateCheckoutAttributeModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeModel model);
        CheckoutAttributeValue InsertCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValueModel model);
        CheckoutAttributeValue UpdateCheckoutAttributeValueModel(CheckoutAttribute checkoutAttribute, CheckoutAttributeValue checkoutAttributeValue, CheckoutAttributeValueModel model);
    }
}
