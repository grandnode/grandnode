using Grand.Core.Domain.Common;
using Grand.Core.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IReturnRequestViewModelService
    {
        ReturnRequestModel PrepareReturnRequestModel(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties);
        (IList<ReturnRequestModel> returnRequestModels, int totalCount) PrepareReturnRequestModel(ReturnReqestListModel model, int pageIndex, int pageSize);
        void PrepareAddressModel(ref AddressModel model, Address address, bool excludeProperties);
        void NotifyCustomer(ReturnRequest returnRequest);
        ReturnReqestListModel PrepareReturnReqestListModel();
        IList<ReturnRequestModel.ReturnRequestItemModel> PrepareReturnRequestItemModel(string returnRequestId);
        ReturnRequest UpdateReturnRequestModel(ReturnRequest returnRequest, ReturnRequestModel model, string customAddressAttributes);
        void DeleteReturnRequest(ReturnRequest returnRequest);
    }
}
