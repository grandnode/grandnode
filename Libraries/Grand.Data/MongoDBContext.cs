using MongoDB.Driver;

namespace Grand.Data
{
    public class MongoDBContext : IMongoDBContext
    {
        protected IMongoDatabase _database;
        protected IMongoClient _client;

        public MongoDBContext()
        {

        }
        public MongoDBContext(string connectionString)
        {
            _client = new MongoClient(connectionString);
            var databaseName = new MongoUrl(connectionString).DatabaseName;
            _database = _client.GetDatabase(databaseName);
        }

        public IMongoClient Client()
        {
            return _client;
        }

        public IMongoDatabase Database()
        {
            return _database;
        }

        public TResult RunCommand<TResult>(string command)
        {
            return _database.RunCommand<TResult>(command);
        }

        public TResult RunCommand<TResult>(string command, ReadPreference readpreference)
        {
            return _database.RunCommand<TResult>(command, readpreference);
        }
    }
}
