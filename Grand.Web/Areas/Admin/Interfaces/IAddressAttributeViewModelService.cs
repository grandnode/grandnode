using Grand.Domain.Common;
using Grand.Web.Areas.Admin.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IAddressAttributeViewModelService
    {
        Task<(IEnumerable<AddressAttributeModel> addressAttributes, int totalCount)> PrepareAddressAttributes();
        AddressAttributeModel PrepareAddressAttributeModel();
        AddressAttributeModel PrepareAddressAttributeModel(AddressAttribute addressAttribute);
        Task<AddressAttribute> InsertAddressAttributeModel(AddressAttributeModel model);
        Task<AddressAttribute> UpdateAddressAttributeModel(AddressAttributeModel model, AddressAttribute addressAttribute);
        Task<(IEnumerable<AddressAttributeValueModel> addressAttributeValues, int totalCount)> PrepareAddressAttributeValues(string addressAttributeId);
        AddressAttributeValueModel PrepareAddressAttributeValueModel(string addressAttributeId);
        Task<AddressAttributeValue> InsertAddressAttributeValueModel(AddressAttributeValueModel model);
        AddressAttributeValueModel PrepareAddressAttributeValueModel(AddressAttributeValue addressAttributeValue);
        Task<AddressAttributeValue> UpdateAddressAttributeValueModel(AddressAttributeValueModel model, AddressAttributeValue addressAttributeValue);

    }
}
