using Grand.Core.Infrastructure;
using Grand.Services.Logging;
using System;
using System.Linq;

namespace Grand.Services.Events
{
    /// <summary>
    /// Evnt publisher
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public EventPublisher()
        {
        }
        #region Methods

        /// <summary>
        /// Publish event to consumers
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="eventMessage">Event message</param>
        public virtual void Publish<T>(T eventMessaget)
        {
            //get all event consumers
            var consumers = EngineContext.Current.ResolveAll<IConsumer<T>>().ToList();

            foreach (var consumer in consumers)
            {
                try
                {
                    consumer.HandleEvent(eventMessaget);
                }
                catch (Exception exception)
                {
                    try
                    {
                        EngineContext.Current.Resolve<ILogger>()?.Error(exception.Message, exception);
                    }
                    catch { }
                }
            }
        }

        #endregion

    }
}
