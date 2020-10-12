using Grand.Domain.Orders;
using Grand.Services.Configuration;
using Grand.Services.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Tasks
{
    public partial class CancelOrderScheduledTask : IScheduleTask
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;

        public CancelOrderScheduledTask(ISettingService settingService, IOrderService orderService)
        {
            _settingService = settingService;
            _orderService = orderService;
        }

        public async Task Execute()
        {
            var orderSettings = _settingService.LoadSetting<OrderSettings>(string.Empty);
            if (!orderSettings.DaysToCancelUnpaidOrder.HasValue)
                return;

            DateTime startCancelDate = CalculateStartCancelDate(orderSettings.DaysToCancelUnpaidOrder.Value);
            await _orderService.CancelExpiredOrders(startCancelDate);
        }

        private DateTime CalculateStartCancelDate(int daysToCancelUnpaidOrder)
        {
            return DateTime.UtcNow.Date.AddDays(-daysToCancelUnpaidOrder);
        }
    }
}
