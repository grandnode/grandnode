using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.ScheduledJob
{
    public interface IScheduledJobService : IHostedService, IDisposable
    {
        Task ScheduleAsync(CancellationToken cancellationToken);
    }
}
