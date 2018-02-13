using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Tasks;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Orders;
using System;
using System.Linq;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task end auctions
    /// </summary>
    public partial class EndAuctionsTask : ScheduleTask, IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly IAuctionService _auctionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ILogger _logger;
        private readonly object _lock = new object();

        public EndAuctionsTask(IProductService productService, IAuctionService auctionService, IQueuedEmailService queuedEmailService,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationService, IShoppingCartService shoppingCartService,
            ICustomerService customerService, ILogger logger)
        {
            this._productService = productService;
            this._auctionService = auctionService;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationService;
            this._shoppingCartService = shoppingCartService;
            this._customerService = customerService;
            this._logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            lock (_lock)
            {
                var auctionsToEnd = _auctionService.GetAuctionsToEnd();
                foreach (var auctionToEnd in auctionsToEnd)
                {
                    var bid = _auctionService.GetBidsByProductId(auctionToEnd.Id).OrderByDescending(x => x.Amount).FirstOrDefault();
                    if (bid == null)
                        throw new ArgumentNullException("bid");

                    var warnings = _shoppingCartService.AddToCart(_customerService.GetCustomerById(bid.CustomerId), bid.ProductId, Core.Domain.Orders.ShoppingCartType.Auctions,
                        bid.StoreId, customerEnteredPrice: bid.Amount);

                    if (!warnings.Any())
                    {
                        bid.Win = true;
                        _auctionService.UpdateBid(bid);
                        _workflowMessageService.SendAuctionEndedStoreOwnerNotification(auctionToEnd, _localizationSettings.DefaultAdminLanguageId, bid);
                        _workflowMessageService.SendAuctionEndedCustomerNotificationWin(auctionToEnd, null, bid);
                        _workflowMessageService.SendAuctionEndedCustomerNotificationLost(auctionToEnd, null, bid);
                        _auctionService.UpdateAuctionEnded(auctionToEnd, true);
                    }
                    else
                    {
                        _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, $"EndAuctionTask - Product {auctionToEnd.Name}", string.Join(",", warnings.ToArray()));
                        throw new ArgumentNullException($"EndAuctionTask - Product: {auctionToEnd.Name} - {string.Join(", ", warnings.ToArray())}");
                    }
                }
            }
        }
    }
}