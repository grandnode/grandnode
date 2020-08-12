using Grand.Domain.Orders;
using Grand.Services.Configuration;
using Grand.Services.Orders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Grand.Services.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.ScheduledJob
{
    public class ScheduledCancelOrderService : IScheduledJobService
    {
        private System.Timers.Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public ScheduledCancelOrderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleAsync(cancellationToken);
        }

        public Task ScheduleAsync(CancellationToken cancellationToken)
        {
            var nowDateTime = DateTime.UtcNow;
            var nextEventDateTime = nowDateTime.Date.AddDays(1);
            var milliSecondsToFireEvent = (int)((nextEventDateTime - nowDateTime).TotalMilliseconds);

            if (milliSecondsToFireEvent < 1)
                milliSecondsToFireEvent = 10;

            _timer = new System.Timers.Timer(milliSecondsToFireEvent);

            _timer.Elapsed += async (sender, args) =>
            {
                _timer.Dispose();
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                    await CancelOrdersAsync(nowDateTime);

                if (!cancellationToken.IsCancellationRequested)
                    await ScheduleAsync(cancellationToken);
            };
            _timer.Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task CancelOrdersAsync(DateTime todayDate)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _serviceProvider = scope.ServiceProvider;
                var logger = _serviceProvider.GetService<ILogger>();
                try
                {
                    // load order setting
                    var _settingService = _serviceProvider.GetRequiredService<ISettingService>();
                    var orderSetting = _settingService.LoadSetting<OrderSettings>(string.Empty);
                    if (!orderSetting.DaysToCancelUnpaidOrder.HasValue)
                        return;

                    int DaysToCancelUnpaidOrder = orderSetting.DaysToCancelUnpaidOrder.Value;
                    DateTime startCancelDate = todayDate.Date.AddDays(-1 * (DaysToCancelUnpaidOrder - 1)); // -1 avoid query dateTime.Date in Mongo IQueryable 
                    var orderService = _serviceProvider.GetRequiredService<IOrderService>();
                    await orderService.CancelExpiredOrders(startCancelDate);
                }
                catch (Exception exc)
                {
                    await logger.InsertLog(Domain.Logging.LogLevel.Error, $"Error while running the CancelExpiredOrders scheduled task", exc.Message);
                }
            }
        }
    }
}
