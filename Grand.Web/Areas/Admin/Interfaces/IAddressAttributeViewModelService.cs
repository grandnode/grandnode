using Grand.Core.Domain.Common;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IAddressAttributeViewModelService
    {
        (IEnumerable<AddressAttributeModel> addressAttributes, int totalCount) PrepareAddressAttributes();
        AddressAttributeModel PrepareAddressAttributeModel();
        AddressAttributeModel PrepareAddressAttributeModel(AddressAttribute addressAttribute);
        AddressAttribute InsertAddressAttributeModel(AddressAttributeModel model);
        AddressAttribute UpdateAddressAttributeModel(AddressAttributeModel model, AddressAttribute addressAttribute);
        (IEnumerable<AddressAttributeValueModel> addressAttributeValues, int totalCount) PrepareAddressAttributeValues(string addressAttributeId);
        AddressAttributeValueModel PrepareAddressAttributeValueModel(string addressAttributeId);
        AddressAttributeValue InsertAddressAttributeValueModel(AddressAttributeValueModel model);
        AddressAttributeValueModel PrepareAddressAttributeValueModel(AddressAttributeValue addressAttributeValue);
        AddressAttributeValue UpdateAddressAttributeValueModel(AddressAttributeValueModel model, AddressAttributeValue addressAttributeValue);

    }
}
