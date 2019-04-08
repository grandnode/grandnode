using Grand.Core.Domain.Stores;
using System.Threading.Tasks;

namespace Grand.Core
{
    /// <summary>
    /// Store context
    /// </summary>
    public interface IStoreContext
    {
        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        Store CurrentStore { get; set; }

        /// <summary>
        /// Set the current store by Middleware
        /// </summary>
        /// <returns></returns>
        Task<Store> SetCurrentStore();
    }
}
