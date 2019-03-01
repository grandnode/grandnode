using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    public interface IDistributedRedisCacheExtended
    {
        Task ClearAsync();
        Task RemoveByPatternAsync(string pattern);
    }
}
