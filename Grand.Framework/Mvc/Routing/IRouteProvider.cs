using Microsoft.AspNetCore.Routing;

namespace Grand.Framework.Mvc.Routing
{
    /// <summary>
    /// Route provider
    /// </summary>
    public interface IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        void RegisterRoutes(IEndpointRouteBuilder routeBuilder);

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        int Priority { get; }
    }
}
