using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Core.DependencyInjection
{
    /// <summary>
    /// Dependency registrar interface
    /// </summary>
    public interface IDependencyInjection
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="serviceCollection">Service Collection</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config);

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        int Order { get; }
    }
}
