using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IReturnRequestViewModelService
    {
        Task<ReturnRequestModel> PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties);
        Task<(IList<ReturnRequestModel> returnRequestModels, int totalCount)> PrepareReturnRequestModel(ReturnReqestListModel model, int pageIndex, int pageSize);
        Task<AddressModel> PrepareAddressModel(AddressModel model, Address address, bool excludeProperties);
        Task NotifyCustomer(ReturnRequest returnRequest);
        ReturnReqestListModel PrepareReturnReqestListModel();
        Task<IList<ReturnRequestModel.ReturnRequestItemModel>> PrepareReturnRequestItemModel(string returnRequestId);
        Task<ReturnRequest> UpdateReturnRequestModel(ReturnRequest returnRequest, ReturnRequestModel model, string customAddressAttributes);
        Task DeleteReturnRequest(ReturnRequest returnRequest);
        Task<IList<ReturnRequestModel.ReturnRequestNote>> PrepareReturnRequestNotes(ReturnRequest returnRequest);
        Task InsertReturnRequestNote(ReturnRequest returnRequest, Order order, string downloadId, bool displayToCustomer, string message);
        Task DeleteReturnRequestNote(ReturnRequest returnRequest, string id);
    }
}
