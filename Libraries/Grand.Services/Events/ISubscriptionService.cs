using System.Collections.Generic;

namespace Grand.Services.Events
{
    /// <summary>
    /// Event subscription service
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Get subscriptions
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Event consumers</returns>
        IList<IConsumer<T>> GetSubscriptions<T>();
    }
}
