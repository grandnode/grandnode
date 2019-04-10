using Grand.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    /// <summary>
    /// Evnt publisher
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// Ctor
        /// </summary>
        public EventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        #region Methods

        /// <summary>
        /// Publish event to consumers
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="eventMessage">Event message</param>
        public virtual async Task Publish<T>(T eventMessaget)
        {
            var consumers = _serviceProvider.GetServices<IConsumer<T>>().ToList();
            foreach (var consumer in consumers)
            {
                try
                {
                    await consumer.HandleEvent(eventMessaget);
                }
                catch (Exception exception)
                {
                    try
                    {
                        _serviceProvider.GetRequiredService<ILogger>()?.Error(exception.Message, exception);
                    }
                    catch { }
                }
            }

        }

        #endregion

    }
}
