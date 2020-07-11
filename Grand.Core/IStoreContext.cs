using Grand.Domain.Stores;
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
        Store CurrentStore { get; }

        /// <summary>
        /// Set the current store by Middleware
        /// </summary>
        /// <returns></returns>
        Task<Store> SetCurrentStore();

        /// <summary>
        /// Set store cookie
        /// </summary>
        /// <param name="storeId">Store ident</param>
        Task SetStoreCookie(string storeId);
    }
}
