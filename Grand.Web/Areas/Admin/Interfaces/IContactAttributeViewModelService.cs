using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IContactAttributeViewModelService
    {
        Task<IEnumerable<ContactAttributeModel>> PrepareContactAttributeListModel();
        Task PrepareConditionAttributes(ContactAttributeModel model, ContactAttribute contactAttribute);
        Task<ContactAttribute> InsertContactAttributeModel(ContactAttributeModel model);
        Task<ContactAttribute> UpdateContactAttributeModel(ContactAttribute contactAttribute, ContactAttributeModel model);
        ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute);
        ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue);
        Task<ContactAttributeValue> InsertContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValueModel model);
        Task<ContactAttributeValue> UpdateContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue, ContactAttributeValueModel model);
    }
}
