using Grand.Core.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IContactAttributeViewModelService
    {
        IEnumerable<ContactAttributeModel> PrepareContactAttributeListModel();
        void PrepareConditionAttributes(ContactAttributeModel model, ContactAttribute contactAttribute);
        ContactAttribute InsertContactAttributeModel(ContactAttributeModel model);
        ContactAttribute UpdateContactAttributeModel(ContactAttribute contactAttribute, ContactAttributeModel model);
        ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute);
        ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue);
        ContactAttributeValue InsertContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValueModel model);
        ContactAttributeValue UpdateContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue, ContactAttributeValueModel model);
    }
}
