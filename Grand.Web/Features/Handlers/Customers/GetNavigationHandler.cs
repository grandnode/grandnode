using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Vendors;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetNavigationHandler : IRequestHandler<GetNavigation, CustomerNavigationModel>
    {

        private readonly CustomerSettings _customerSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly VendorSettings _vendorSettings;

        public GetNavigationHandler(
            CustomerSettings customerSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            VendorSettings vendorSettings)
        {
            _customerSettings = customerSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _orderSettings = orderSettings;
            _vendorSettings = vendorSettings;
        }

        public async Task<CustomerNavigationModel> Handle(GetNavigation request, CancellationToken cancellationToken)
        {
            var model = new CustomerNavigationModel();
            model.HideRewardPoints = !_rewardPointsSettings.Enabled;
            model.HideDeleteAccount = !_customerSettings.AllowUsersToDeleteAccount;
            model.HideReturnRequests = !_orderSettings.ReturnRequestsEnabled;
            model.HideDownloadableProducts = _customerSettings.HideDownloadableProductsTab;
            model.HideBackInStockSubscriptions = _customerSettings.HideBackInStockSubscriptionsTab;
            model.HideAuctions = _customerSettings.HideAuctionsTab;
            model.HideNotes = _customerSettings.HideNotesTab;
            model.HideDocuments = _customerSettings.HideDocumentsTab;
            model.HideReviews = _customerSettings.HideReviewsTab;
            model.HideCourses = _customerSettings.HideCoursesTab;
            model.HideSubAccounts = _customerSettings.HideSubAccountsTab || !request.Customer.IsOwner();
            if (_vendorSettings.AllowVendorsToEditInfo && request.Vendor != null)
            {
                model.ShowVendorInfo = true;
            }
            model.SelectedTab = (CustomerNavigationEnum)request.SelectedTabId;

            return await Task.FromResult(model);
        }
    }
}
