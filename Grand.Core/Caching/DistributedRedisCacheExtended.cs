using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Core.Caching
{
    public partial class DistributedRedisCacheExtended : IDistributedRedisCacheExtended, IDisposable
    {
        private readonly IDatabase _db;

        private static RedisCacheOptions _options;

        private static ConfigurationOptions GetConnectionOptions()
        {
            ConfigurationOptions redisConnectionOptions = (_options.ConfigurationOptions != null)
                ? ConfigurationOptions.Parse(_options.ConfigurationOptions.ToString())
                : ConfigurationOptions.Parse(_options.Configuration);

            redisConnectionOptions.AbortOnConnectFail = false;

            return redisConnectionOptions;
        }

        public static ConnectionMultiplexer Connection {
            get {
                return lazyConnection.Value;
            }
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(GetConnectionOptions());
        });


        public DistributedRedisCacheExtended(IOptions<RedisCacheOptions> redisCacheOptions)
        {
            _options = redisCacheOptions.Value;
            _db = lazyConnection.Value.GetDatabase();
        }

        public async Task ClearAsync()
        {
            foreach (var endPoint in Connection.GetEndPoints())
            {
                var server = Connection.GetServer(endPoint);
                var keys = server.Keys(_db.Database).ToArray();
                await _db.KeyDeleteAsync(keys);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            foreach (var endPoint in Connection.GetEndPoints())
            {
                var server = Connection.GetServer(endPoint);
                var keys = server.Keys(_db.Database, $"*{pattern}*");
                await _db.KeyDeleteAsync(keys.ToArray());
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
