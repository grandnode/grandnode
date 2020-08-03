using Grand.Domain.Orders;
using Grand.Services.Configuration;
using Grand.Services.Orders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.ScheduledJob
{
    public class ScheduledDeleteOrderService : IScheduledJobService
    {
        private System.Timers.Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public ScheduledDeleteOrderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleAsync(cancellationToken);
        }

        public  Task ScheduleAsync(CancellationToken cancellationToken)
        {
            try
            {
                var todayDateTime = DateTime.UtcNow;
                var firEventDateTime = todayDateTime.Date.AddMinutes(3);
                var milliSecondsToFireEvent = (int)((firEventDateTime - todayDateTime).TotalMilliseconds);
                _timer = new System.Timers.Timer(milliSecondsToFireEvent);

                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                        await DeleteOrdersAsync(todayDateTime, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        await ScheduleAsync(cancellationToken);
                };
                _timer.Start();
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                return Task.CompletedTask;
            }
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

        private async Task DeleteOrdersAsync(DateTime todayDate, CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                // load order setting
                var _settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
                var orderSetting = _settingService.LoadSetting<OrderSettings>(string.Empty);
                if (!orderSetting.DaysToDeleteUnpaidOrder.HasValue)
                    return;

                int DaysToDeleteUnpaidOrder = orderSetting.DaysToDeleteUnpaidOrder.Value;
                DateTime startDeletionDate = todayDate.AddDays(-1 * DaysToDeleteUnpaidOrder);

                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.DeleteExpiredOrders(startDeletionDate);
            }
        }
    }
}
