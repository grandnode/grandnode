using Grand.Core.Configuration;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;

namespace Grand.Core.Caching
{
    /// <summary>
    /// Represents Redis connection wrapper implementation
    /// </summary>
    public class RedisConnectionWrapper : IRedisConnectionWrapper
    {
        #region Fields

        private readonly GrandConfig _config;

        private readonly Lazy<string> _connectionString;
        private volatile ConnectionMultiplexer _connection;
        private volatile RedLockFactory _redLockFactory;
        private readonly object _lock = new object();

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="config">Config</param>
        public RedisConnectionWrapper(GrandConfig config)
        {
            this._config = config;
            this._connectionString = new Lazy<string>(GetConnectionString);
            this._redLockFactory = CreateRedisLockFactory();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get connection string to Redis cache from configuration
        /// </summary>
        /// <returns></returns>
        protected string GetConnectionString()
        {
            return _config.RedisCachingConnectionString;
        }

        /// <summary>
        /// Get connection to Redis servers
        /// </summary>
        /// <returns></returns>
        protected ConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected) return _connection;

            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected) return _connection;

                //Connection disconnected. Disposing connection...
                _connection?.Dispose();

                //Creating new instance of Redis Connection
                _connection = ConnectionMultiplexer.Connect(_connectionString.Value);
            }

            return _connection;
        }

        /// <summary>
        /// Create instance of RedisLockFactory
        /// </summary>
        /// <returns>RedisLockFactory</returns>
        protected RedLockFactory CreateRedisLockFactory()
        {
            //get password and value whether to use ssl from connection string
            var password = string.Empty;
            var useSsl = false;
            foreach (var option in _connectionString.Value.Split(',').Where(option => option.Contains('=')))
            {
                switch (option.Substring(0, option.IndexOf('=')).Trim().ToLowerInvariant())
                {
                    case "password":
                        password = option.Substring(option.IndexOf('=') + 1).Trim();
                        break;
                    case "ssl":
                        bool.TryParse(option.Substring(option.IndexOf('=') + 1).Trim(), out useSsl);
                        break;
                }
            }
            var points = GetEndPoints().Select(endPoint => new RedLockEndPoint
            {
                EndPoint = endPoint,
                Password = password,
                Ssl = useSsl
            }).ToList();

            return RedLockFactory.Create(points);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Obtain an interactive connection to a database inside Redis
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        /// <returns>Redis cache database</returns>
        public IDatabase GetDatabase(int? db = null)
        {
            return GetConnection().GetDatabase(db ?? -1);
        }

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        /// <param name="endPoint">The network endpoint</param>
        /// <returns>Redis server</returns>
        public IServer GetServer(EndPoint endPoint)
        {
            return GetConnection().GetServer(endPoint);
        }

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns>Array of endpoints</returns>
        public EndPoint[] GetEndPoints()
        {
            return GetConnection().GetEndPoints();
        }

        /// <summary>
        /// Delete all the keys of the database
        /// </summary>
        /// <param name="db">Database number; pass null to use the default value</param>
        public void FlushDatabase(int? db = null)
        {
            var endPoints = GetEndPoints();

            foreach (var endPoint in endPoints)
            {
                GetServer(endPoint).FlushDatabase(db ?? -1);
            }
        }


        /// <summary>
        /// Release all resources associated with this object
        /// </summary>
        public void Dispose()
        {
            //dispose ConnectionMultiplexer
            _connection?.Dispose();

            //dispose RedisLockFactory
            _redLockFactory?.Dispose();
        }

        #endregion
    }
}
