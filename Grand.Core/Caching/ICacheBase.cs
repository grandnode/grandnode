using System;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    /// <summary>
    /// Cache manager interface
    /// </summary>
    public interface ICacheBase: IDisposable
    {
        Task<T> GetAsync<T>(string key, Func<Task<T>> acquire);
        T Get<T>(string key, Func<T> acquire);
        Task SetAsync(string key, object data, int cacheTime);
        Task RemoveAsync(string key, bool publisher = true);
        Task RemoveByPrefix(string prefix, bool publisher = true);        
        Task Clear(bool publisher = true);
    }
}
